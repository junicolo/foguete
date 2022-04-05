using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UI : MonoBehaviour {
    [SerializeField] TextMeshProUGUI measures;
    [SerializeField] GameObject pause;
    private Rigidbody track;
    public static UI _;
    private bool showVel;
    private int Layer = 1 << 6; 
    void AlternateData() => showVel = !showVel;
        
    void Awake() {

        Cursor.lockState = menuON ? CursorLockMode.None : CursorLockMode.Locked;
        _ = this;
        InvokeRepeating("AlternateData", 1, 6);
    }

    private bool menuON;
    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            menuON = !menuON;
            Cursor.lockState = menuON ? CursorLockMode.None : CursorLockMode.Locked;
            Time.timeScale = menuON ? 0 : 1;
            pause.SetActive(menuON);
        }

        if (menuON) {
            if (Input.GetKeyDown(KeyCode.Q)) {
                Application.Quit();
            }
            if (Input.GetKeyDown(KeyCode.R)) {
                Time.timeScale = 1;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            return;
        }
        
        if (started) {
            if (showVel) {
                measures.text = $"Vel: {(int)track.velocity.y} km/h";
            } else if (Physics.Raycast(track.position, Vector3.down, out RaycastHit hit, 1000, Layer)) {
                measures.text = $"Alt: {(int)hit.distance} m";
            }
        }
    }

    private bool started;
    public void Measure(Rigidbody track) {
        started = true;
        this.track = track;
    }
}
