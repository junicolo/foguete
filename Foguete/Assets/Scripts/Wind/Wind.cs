using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Wind : MonoBehaviour {
    public static Wind _;
    private float turbulence;
    private void Awake() => _ = this;
    private void Start() { 
        Random.InitState(12345);
        InvokeRepeating("WindZone", 1, 15);
        InvokeRepeating("UpdateTurbulance", 1, 2);
    } 
    

    private Vector3 dir;
    void WindZone() {
        if (Random.value > 0.5f) dir = new Vector3(Random.Range(-1f, 1f),0,Random.Range(-1f, 1f));
    }

    void UpdateTurbulance() => turbulence = Random.value;
    public Vector3 WindForce() => turbulence * dir;

}
