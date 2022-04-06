using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum EngineID {
    engine0, engine1    
}

public class RocketManager : MonoBehaviour {

    private bool liftoff;
    
    Rigidbody rb;
    Dictionary<EngineID, Engine> engines = new Dictionary<EngineID, Engine>();
    private Engine secEngine;

    [SerializeField] CinemachineFreeLook cam;
    [SerializeField] Transform camTarget;
    private Vector3 target;
    

    private void Start() {
        rb = GetComponent<Rigidbody>();
        Engine[] thrusters = GetComponentsInChildren<Engine>();
        target = camTarget.localPosition;
        foreach (Engine thruster in thrusters)
            engines.Add(thruster.engineId, thruster);
    }

    /// <summary>
    /// Aguarda o comando para iniciar a propulsão do motor.
    /// </summary>
    public void Update() {
        if (!liftoff && Input.GetKeyDown(KeyCode.Return)) {
            liftoff = true;
            engines[0].Ignition(5);
            UI._.Measure(rb);
        }
    }

    /// <summary>
    /// Recebe a informação que um motor foi desligado.
    /// </summary>
    /// <param name="id">Identificador do motor desligado.</param>
    public void EngineCutoff(EngineID id) {
        engines[id].StageSeparation();
        if (id == EngineID.engine0) StartCoroutine(SecStageIgnition());

        StartCoroutine(ParachuteDeploy(engines[id]));
    }

    IEnumerator SecStageIgnition() {
        camTarget.position = cam.Follow.position;
        cam.Follow = camTarget;
        cam.LookAt = camTarget;
        
        while (Vector3.Distance(camTarget.localPosition, target) > 1 ) { // transição da camera para acompanhar o segundo estagio.
            camTarget.localPosition = Vector3.Lerp(camTarget.localPosition, target, 0.1f);
            yield return new WaitForEndOfFrame();
        }
        
        yield return new WaitUntil(() => rb.velocity.y < 0); // quando o foguete for começar a perder altitude, o motor do segundo estagio é acionado
        engines[EngineID.engine1].Ignition(2);
    }

    /// <summary>
    /// Começa as intruções para lançar o paraquedas do motor.
    /// </summary>
    /// <param name="thruster"></param>
    /// <returns></returns>
    IEnumerator ParachuteDeploy(Engine thruster) {
        Rigidbody stage = thruster.GetRb;
        yield return new WaitUntil(() =>  stage.velocity.y < 10);
        thruster.SmoothDrag = false;
        thruster.PrepareLand();
    }
}
