using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Parachute : MonoBehaviour {
    
    [SerializeField] [Range(0.01f, 1)] private float rotSpeed = .2f;
    public Vector3 stageRotationTarget;
    public float dragForce;

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
            stage.rotation = Quaternion.Slerp(stage.rotation, groundDifference, rotSpeed);
            var angleDifference = Quaternion.Angle(stage.rotation, groundDifference);
            
            if (!deployed && angleDifference < 5) {
                StartCoroutine(Deploy());
            } else if (angleDifference < 0) {
                maneuvering = false;
            }            
        }
    }

    public void LandManeuver(Rigidbody stage) {
        gameObject.SetActive(true);

        GetComponent<SkinnedMeshRenderer>().enabled = false;
        this.stage = stage;
        maneuvering = true;
        StartCoroutine(SmoothFlip());
    }

    IEnumerator Deploy () {
        deployed = true;
        yield return new WaitForSeconds(2);

        GetComponent<SkinnedMeshRenderer>().enabled = true;
        SoundManager.PlaySound(Sound.Parachute, transform);
        openParachute = true;
    }

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
