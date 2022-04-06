using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour {
    
    public static UI _;
    [SerializeField] TextMeshProUGUI measures;
    [SerializeField] GameObject pause;
    
    private Rigidbody track;
    private int Layer = 1 << 6; 
    
    private bool started;
    private bool menuON; // pause Menu button
    private bool showVel;
        
    void Awake() {
        Cursor.lockState = menuON ? CursorLockMode.None : CursorLockMode.Locked;
        _ = this;
        InvokeRepeating("AlternateData", 1, 6); 
    }
    
    /// <summary>
    /// Altera entre velocidade e altura na UI a cada 6 segundos.
    /// </summary>
    void AlternateData() => showVel = !showVel;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menuON = !menuON;
            Cursor.lockState = menuON ? CursorLockMode.None : CursorLockMode.Locked;
            Time.timeScale = menuON ? 0 : 1;
            pause.SetActive(menuON);
        }

        if (menuON) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                Application.Quit();
            }
            if (Input.GetKeyDown(KeyCode.R)) {
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }
        
        if (started) {
            if (showVel) {
                measures.text = $"Vel: {(int)track.velocity.y} km/h";
            } else if (Physics.Raycast(track.position, Vector3.down, out RaycastHit hit, 1000, Layer)) {
                measures.text = $"Alt: {(int)hit.distance} m"; // exibe distancia do solo.
            }
        }
    }

    
    /// <summary>
    /// Recebe um rigidbody para capturar valores como velocidade e altura para exibir na UI
    /// </summary>
    /// <param name="track">Estagio do foguete que os dados são exibidos.</param>
    public void Measure(Rigidbody track) {
        started = true;
        this.track = track;
    }
}
