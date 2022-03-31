using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
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
    private float debugTime;
    private float oldt = 1;
    private Vector3 centerOfMass;
    private bool onAir;
    private Wind wind;

    private bool smoothDrag;
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
           
            debugTime += Time.deltaTime;
            if (debugTime > oldt) {
                print(oldt + "sec : " + rb.velocity.y);
                oldt++;
            }
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
        StartCoroutine(EngineCutoff(t));
    }

    IEnumerator EngineCutoff(float burnTime) {
        yield return new WaitForSeconds(burnTime);
        burning = false;
        yield return new WaitUntil(() => rb.velocity.y < 20);
        rocket.EngineCutoff(engineId);
    }

    public void StageSeparation() {
        transform.parent = null;
        Vector3 rocketVel = rb.velocity;

        var engines = Enum.GetValues(typeof(EngineID));

        if (engineId != (EngineID) engines.GetValue(engines.Length - 1)) 
            rb.AddForce(rb.transform.up * 1f,ForceMode.Impulse);

        rb = gameObject.AddComponent<Rigidbody>();
        rb.velocity = rocketVel;
    }


    private void OnCollisionEnter(Collision collision) {
        if(!onAir) return;

        print(collision.collider.name);
    }


}
