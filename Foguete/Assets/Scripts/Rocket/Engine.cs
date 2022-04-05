using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Engine : MonoBehaviour {
    /// <summary>
    /// Massa do Foguete: 9.81 N = ≈ 1 KG
    /// Aceleração Desejada: 10 kg-m/s => 98.1 N
    /// Δt = ≈ 0.1s
    /// </summary>

    public EngineID engineId;
    
    private RocketManager rocket;

    private Rigidbody rb;
    private Parachute parachute;
    
    private bool burning;
    
    private Vector3 centerOfMass;
    private bool onAir;
    private Wind wind;

    private bool smoothDrag;

    [SerializeField] private GameObject thrusterEmitter;
    [SerializeField] private Transform groundEmitter;
    [SerializeField] private ParticleSystemForceField groundEmitterEffector;
    private const int Layer = 1 << 6;
    private bool groundDust;
    
    public bool SmoothDrag {
        set => smoothDrag = value;
    }

    public Rigidbody GetRb => rb;
    public void PrepareLand() => parachute.LandManeuver(rb);
    private void Awake() {
        rb = (rocket = transform.parent.GetComponent<RocketManager>()).GetComponent<Rigidbody>();
        rb.isKinematic = true;
        parachute = GetComponentInChildren<Parachute>();
        parachute.gameObject.SetActive(false);
    }

    private void Start() => wind = Wind._;
    
    private void FixedUpdate() {
        if (burning) {
            rb.AddForce(10 * Time.deltaTime * rb.transform.up * 98 , ForceMode.Acceleration);
            HeightParticles();

        } else if (smoothDrag) {
           rb.drag = .03f;
        }

        if (onAir) {
            Vector3 windDir = wind.WindForce();
            rb.AddForce(windDir);
        }
    }

    public void Ignition(float t) {
        burning = true;
        smoothDrag = true;
        onAir = true;
        rb.isKinematic = false;
        thrusterEmitter.SetActive(true);
        SoundManager._.PlaySequence(Sound.Ignition, transform);
        StartCoroutine(EngineCutoff(t));
    }

    IEnumerator EngineCutoff(float burnTime) {
        yield return new WaitForSeconds(burnTime);
        burning = false;
        thrusterEmitter.SetActive(false);
        SoundManager._.StopSequence(Sound.Ignition);
        yield return new WaitUntil(() => rb.velocity.y < 20);

        rocket.EngineCutoff(engineId);
    }

    public void StageSeparation() {
        transform.parent = null;
        Vector3 rocketVel = rb.velocity;

        var engines = Enum.GetValues(typeof(EngineID));

        if (engineId != (EngineID) engines.GetValue(engines.Length - 1)) { 
            SoundManager.PlaySound(Sound.Separation, rb.transform);            
            rb.AddForce(rb.transform.up * 1f,ForceMode.Impulse);
        }

        rb = gameObject.AddComponent<Rigidbody>();
        rb.velocity = rocketVel;

        if (engineId == (EngineID) engines.GetValue(engines.Length - 1)) {
            UI._.Measure(rb);
        }
    }

    private void OnCollisionEnter(Collision collision) {
        if(!onAir) return;
        if (collision.collider.gameObject.layer == 6) {
            parachute.gameObject.SetActive(false);
            rb.angularDrag = 200;
        };
    }

    void HeightParticles() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 15, Layer)) {
            if (hit.distance < 10 && !groundDust) {
                groundDust = true;
                var emissionModule = groundEmitter.GetComponent<ParticleSystem>().emission;
                emissionModule.rateOverTimeMultiplier = 100;    
            } else if (groundDust && hit.distance > 10) {
                var emissionModule = groundEmitter.GetComponent<ParticleSystem>().emission;
                emissionModule.rateOverTimeMultiplier = 0;
                groundDust = false;
            }
            
            if(!groundDust) return;            
            groundEmitter.transform.position = hit.point;
            groundEmitterEffector.transform.position = hit.point;
            groundEmitterEffector.gravity = Mathf.Lerp(0,-5,Mathf.InverseLerp(10,0, hit.distance));
            
        }
    }


}
