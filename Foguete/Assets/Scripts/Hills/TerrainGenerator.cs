using System.IO;
using UnityEngine;
using System.Linq;

public class TerrainGenerator : MonoBehaviour{
    public Terrain sampleT;
    public Texture2D heightmap;
    public Texture2D grassL;
    public Texture2D grassT;
    private float[,] heights;
    private int tSize;
    
    private void Start() {
        tSize = sampleT.terrainData.heightmapResolution;
        sampleT.heightmapMaximumLOD = 0;

        TerrainLayer[] layers = new TerrainLayer[2];
        layers[0] = new TerrainLayer {
            diffuseTexture = grassL,
            tileSize =  new Vector2(8,8)   
        };

        layers[1] = new TerrainLayer {
            diffuseTexture = grassT,
            tileSize =  new Vector2(16,36)
        };

        sampleT.terrainData.terrainLayers = layers;
    }

    /// <summary>
    /// Utiliza a informação da normal do terreno para texturizar de acordo com a inclinação 
    /// <see cref="https://alastaira.wordpress.com/2013/11/14/procedural-terrain-splatmapping/"/>
    /// </summary>
    void PaintScene() {
        TerrainData terrainData = sampleT.terrainData; 
        float[, ,] splatmapData = new float[terrainData.alphamapWidth, terrainData.alphamapHeight, terrainData.alphamapLayers];

        for (int y = 0; y < sampleT.terrainData.alphamapHeight; y++){
            for (int x = 0; x < sampleT.terrainData.alphamapWidth; x++) {

                // Normaliza x/y para pegar a altura respectiva as coordenadas 
                float idxVer = (float) y / terrainData.alphamapHeight;
                float idxHor = (float) x / terrainData.alphamapWidth; 
                float height = terrainData.GetHeight(Mathf.RoundToInt(idxVer * terrainData.heightmapResolution),Mathf.RoundToInt(idxHor * terrainData.heightmapResolution) );
              
                
                Vector3 normal = terrainData.GetInterpolatedNormal(idxVer,idxHor); // pega Normal (dir q aponta a UV) do terreno
  
                float[] splatWeights = new float[terrainData.alphamapLayers]; // array para aplicação de cores no texel
                splatWeights[0] = Mathf.Clamp01(terrainData.heightmapResolution - height); 
                splatWeights[1] = height * (normal.x + normal.z); 
                float z = splatWeights.Sum();

                for(int i = 0; i < terrainData.alphamapLayers; i++){
                    splatWeights[i] /= z;
                    splatmapData[x, y, i] = splatWeights[i];
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    /// <summary>
    /// Carrega a altura de cada texel usando a informação contida em cada pixel de uma imagem. 
    /// </summary>
    void LoadHeightmap() {
        heights = sampleT.terrainData.GetHeights(0, 0, tSize, tSize);
        for (int x = 0; x < tSize; x++) {
            for (int z = 0; z < tSize; z++) 
                heights[x, z] = Mathf.Lerp(0.00f, 0.08649209f, heightmap.GetPixel(x, z).r); 
        }
        sampleT.terrainData.SetHeights(0,0,heights);
    }
    
    /// <summary>
    /// Salva a altura de cada Texel em um pixel numa imagem relativa ao tamanho do terreno.
    /// </summary>
    void SaveHeightmap() {
        heights = sampleT.terrainData.GetHeights(0, 0, tSize, tSize);
        var texture = new Texture2D(tSize, tSize, TextureFormat.ARGB32, false);

        for (int x = 0; x < tSize; x++) {
            for (int z = 0; z < tSize; z++) {
                float col = Mathf.InverseLerp(0.00f, 0.08649209f, heights[x, z]); //min 0 | max 0.08649209f
                texture.SetPixel(x, z, new Color(col,col,col,1));
            }
        }

        texture.Apply();
        File.WriteAllBytes(Application.persistentDataPath + "/map.png", texture.EncodeToPNG());
        Debug.Log(Application.persistentDataPath);
    }
}
