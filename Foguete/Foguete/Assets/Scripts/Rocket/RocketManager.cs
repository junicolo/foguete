using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public enum EngineID {
    engine0, engine1    
}

public class RocketManager : MonoBehaviour {

    Dictionary<EngineID, Engine> engines = new Dictionary<EngineID, Engine>();
    [SerializeField] private TextMeshProUGUI uiRocketInfo;

    private bool liftoff;
    private Engine secEngine;
    Rigidbody rb;

    private void Start() {
        rb = GetComponent<Rigidbody>();
        Engine[] thrusters = GetComponentsInChildren<Engine>();
        
        for (int i = 0; i < thrusters.Length; i++) {
            Engine thruster = thrusters[i];
            engines.Add(thruster.engineId, thruster);
        }
    }

    public void Update() {
        if (!liftoff && Input.GetKeyDown(KeyCode.Return)) {
            engines[0].Ignition(5);
        }
        
        uiRocketInfo.text = $"Vel {(int) rb.velocity.y} Km/h ↨ Alt {(int) transform.position.y} m";
    }

    public void EngineCutoff(EngineID id) {
        
        if (id == EngineID.engine0) {
            engines[id].StageSeparation();
            StartCoroutine(SecStageIgnition());
        } else {
            print("Second Stage cutoff");            
        }

        StartCoroutine(ParachuteDeploy(engines[id]));
    }

    IEnumerator SecStageIgnition() {
        yield return new WaitUntil(() => rb.velocity.y < 0);
        engines[EngineID.engine1].Ignition(2);
    }


    IEnumerator ParachuteDeploy(Engine thruster) {
        Rigidbody stage = thruster.rb;
        print($"{thruster.name} falling... {Mathf.Abs(stage.velocity.y)}");
        yield return new WaitUntil(() =>  Mathf.Abs(stage.velocity.y) > 10);
        
        try {
            thruster.parachute.Deploy(stage);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

    }
}
