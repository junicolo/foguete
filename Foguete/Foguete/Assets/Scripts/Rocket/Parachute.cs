using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parachute : MonoBehaviour {
    
    private bool drag;
    private Rigidbody stage;

    private void Start() {
        gameObject.SetActive(false);
    }

    private void Update() {
        if (drag) {

            if (stage.velocity.y < -2f) {
                stage.AddForceAtPosition( Vector3.up * 0.352f , Vector3.forward, ForceMode.Force);
                    
            }
        }
    }

    public void Deploy(Rigidbody stage) {
        gameObject.SetActive(true);
        this.stage = stage;
        

    }
    
}
