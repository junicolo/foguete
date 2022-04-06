using System;
using System.Collections;
using UnityEngine;

public class Parachute : MonoBehaviour {
    
    [SerializeField] [Range(0.01f, 1)] private float rotSpeed = .2f; // velocidade que o foguete aponta para baixo
    public Vector3 stageRotationTarget; // direção para qual lado o foguete deve apontar
    public float dragForce; // força do paraquedas

    private Quaternion groundDifference;
    
    private Cloth parachuteCloth;
    private Wind wind;
    private Rigidbody stage;

    private bool maneuvering;
    private bool deployed;
    private bool openParachute;

    private void Start() {
        parachuteCloth = GetComponent<Cloth>();
        wind = Wind._;
    }

    private void FixedUpdate() {
        if (openParachute) {
            stage.drag = dragForce;
            parachuteCloth.externalAcceleration = wind.WindForce();
        }

        if (maneuvering) {
            
            var position = stage.position;
            
            groundDifference = Quaternion.LookRotation((position - stageRotationTarget - position).normalized);
            stage.rotation = Quaternion.Slerp(stage.rotation, groundDifference, rotSpeed); // faz o estagio manobrar para apontar na direção certa 
            
            var angleDifference = Quaternion.Angle(stage.rotation, groundDifference); 

            if (!deployed && angleDifference < 5) { 
                StartCoroutine(Deploy()); // quando o foguete apontar para a direção certa ele abre o paraquedas
            } else if (angleDifference < 0) {
                maneuvering = false;
            }            
        }
    }

    public void LandManeuver(Rigidbody stage) {
        gameObject.SetActive(true);

        GetComponent<SkinnedMeshRenderer>().enabled = false;
        this.stage = stage;
        
        maneuvering = true; // começa a manobrar para abrir o paraquedas
        StartCoroutine(SmoothFlip());
    }

    /// <summary>
    /// Abre o paraquedas
    /// </summary>
    /// <returns></returns>
    IEnumerator Deploy () {
        deployed = true;
        yield return new WaitForSeconds(2);

        GetComponent<SkinnedMeshRenderer>().enabled = true;
        SoundManager.PlaySound(Sound.Parachute, transform);
        openParachute = true;
    }
    /// <summary>
    /// Aplica uma suavização na queda durante à manobra
    /// </summary>
    /// <returns></returns>
    IEnumerator SmoothFlip() {
        while (!deployed) {
            stage.drag += 0.1f;
            yield return new WaitForFixedUpdate();
        }

        while (stage.drag > 0) {
            stage.drag -= 0.1f;
            yield return new WaitForFixedUpdate();
        }
    }
}
