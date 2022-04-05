using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public enum EngineID {
    engine0, engine1    
}

public class RocketManager : MonoBehaviour {

    [SerializeField] CinemachineFreeLook cam;
    [SerializeField] Transform camTarget;
    private Vector3 target;
    
    Dictionary<EngineID, Engine> engines = new Dictionary<EngineID, Engine>();
    private bool liftoff;
    private Engine secEngine;
    Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        Engine[] thrusters = GetComponentsInChildren<Engine>();
        target = camTarget.localPosition;
        foreach (Engine thruster in thrusters)
            engines.Add(thruster.engineId, thruster);
    }

    public void Update() {
        if (!liftoff && Input.GetKeyDown(KeyCode.Return)) {
            liftoff = true;
            engines[0].Ignition(5);
            UI._.Measure(rb);
        }
    }

    public void EngineCutoff(EngineID id) {
        engines[id].StageSeparation();
        if (id == EngineID.engine0) StartCoroutine(SecStageIgnition());

        StartCoroutine(ParachuteDeploy(engines[id]));
    }

    IEnumerator SecStageIgnition() {
        camTarget.position = cam.Follow.position;
        cam.Follow = camTarget;
        cam.LookAt = camTarget;
        
        
        while (Vector3.Distance(camTarget.localPosition, target) > 1 ) { // transição suave para a camera acompanhar o segundo estagio.
            camTarget.localPosition = Vector3.Lerp(camTarget.localPosition, target, 0.1f);
            yield return new WaitForEndOfFrame();
        }
        yield return new WaitUntil(() => rb.velocity.y < 0);
        engines[EngineID.engine1].Ignition(2);
    }


    IEnumerator ParachuteDeploy(Engine thruster) {
        Rigidbody stage = thruster.GetRb;
        yield return new WaitUntil(() =>  stage.velocity.y < 10);
        thruster.SmoothDrag = false;
        thruster.PrepareLand();
    }
}
