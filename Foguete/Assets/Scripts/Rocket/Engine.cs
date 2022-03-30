using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[ExecuteAlways]
public class Engine : MonoBehaviour {
    /// <summary>
    /// Massa do Foguete: 9.81 N = ≈ 1 KG
    /// Aceleração Desejada: 10 kg-m/s => 98.1 N
    /// Δt = ≈ 0.1s
    /// </summary>

    public EngineID engineId;
    [HideInInspector] public Parachute parachute;
    [HideInInspector] public Rigidbody rb;
    
    private RocketManager rocket;
    private bool smoothDrag;
    private bool burning;
    private float debugTime;
    private float oldt = 1;
    private Vector3 centerOfMass;

    private void Start() {
        rb = (rocket = transform.parent.GetComponent<RocketManager>()).GetComponent<Rigidbody>();
        parachute = GetComponentInChildren<Parachute>();
    }

    private void Update() {
        if (burning) {
            rb.AddForce(new Vector3(0,  10 * Time.deltaTime * 98.1f,0) , ForceMode.Acceleration); 
           
            debugTime += Time.deltaTime;
            if (debugTime > oldt) {
                print(oldt + "sec : " + rb.velocity.y);
                oldt++;
            }
        } else if (smoothDrag) {
           rb.drag = .03f;
        }
    }

    public void Ignition(float t) {
        smoothDrag = true;
        burning = true;
        StartCoroutine(EngineCutoff(t));
    }

    IEnumerator EngineCutoff(float burnTime) {
        yield return new WaitForSeconds(burnTime);
        burning = false;
        yield return new WaitUntil(() => rb.velocity.y < 20);
        rocket.EngineCutoff(engineId);
    }

    public void StageSeparation() {
        Vector3 rocketVel = rb.velocity;
        rb.AddForce(Vector3.up * 1f,ForceMode.Impulse);
        rb = gameObject.AddComponent<Rigidbody>();
        rb.velocity = rocketVel;
    } 
    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(rb.centerOfMass,.2f);
    }
}
