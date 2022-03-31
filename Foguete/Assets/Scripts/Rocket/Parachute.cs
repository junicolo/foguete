using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Parachute : MonoBehaviour {
    
    [SerializeField] [Range(0.01f, 1)] private float rotSpeed = .2f;
    public Vector3 stageRotationTarget;
    private Quaternion groundDifference;

    public float dragForce;
    
    private Cloth parachuteCloth;
    private Wind wind;
    private Rigidbody stage;
    private Transform parent;

    private bool maneuvering;
    private bool deployed;
    private bool openParachute;

    private void Start() {
        parachuteCloth = GetComponent<Cloth>();
        wind = Wind._;
        parent = transform.parent;
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
            }else if (angleDifference < 0) {
                maneuvering = false;
            }            
        }
    }

    public void LandManeuver(Rigidbody stage) {
        gameObject.SetActive(true);

        GetComponent<SkinnedMeshRenderer>().enabled = false;
        this.stage = stage;
        parent = transform.parent;
        maneuvering = true;
        StartCoroutine(SmoothFlip());
    }

    IEnumerator Deploy () {
        deployed = true;

        Physics.Raycast(parent.position + Vector3.down * 3, Vector3.down, out RaycastHit hit);
       // float distance = 1;
       // if (hit.collider) {
       //     distance = Vector3.Distance(parent.position, hit.point);
       // }

       // float diveTime = Mathf.InverseLerp(20, 2000, distance);
       // diveTime = Mathf.Lerp(0, 60, diveTime);

      // for (int i = 0; i < Mathf.CeilToInt(diveTime); i++) {
      //     if (parent.position.y - (Math.Abs(stage.velocity.y) + diveTime * ) - distance <= 40) {
      //         print($"({name}) alt: {parent.position.y} dis:{distance} Sf{parent.position.y - (Math.Abs(stage.velocity.y) + diveTime * 9.81f)} res: {distance - (parent.position.y - (Math.Abs(stage.velocity.y) + diveTime * 9.81f)) <= 20}");
      //         diveTime /= 2;
      //     }
      //     else
      //         break;
      // }
        
       // print(name + " "+ diveTime + " " + distance + " " + hit.collider.name);
        
        yield return new WaitForSeconds(2);
        GetComponent<SkinnedMeshRenderer>().enabled = true;
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
    public void OnDrawGizmos() {
        var position = transform.parent.position;
        position -= stageRotationTarget;
        Gizmos.DrawCube( position, Vector3.one * .1f);
    }
}
