using System;
using System.Collections;
using UnityEngine;

public class Engine : MonoBehaviour {

    public EngineID engineId;

    private RocketManager rocket;
    private Rigidbody rb;
    private Parachute parachute;
    
    private bool burning; 
    private bool onAir;
    private bool smoothDrag;
    private Wind wind;

    // Particles e Effects
    [SerializeField] private GameObject thrusterEmitter;
    [SerializeField] private Transform groundEmitter;
    [SerializeField] private ParticleSystemForceField groundEmitterEffector;
    private bool groundDust;
    private const int Layer = 1 << 6;

    // Estado Inicial = Foguete inicia com um angulo, e aguarda comando do RocketManager para decolar.   
    private void Awake() {
        rb = (rocket = transform.parent.GetComponent<RocketManager>()).GetComponent<Rigidbody>();
        rb.isKinematic = true;
        parachute = GetComponentInChildren<Parachute>();
        parachute.gameObject.SetActive(false);
    }
    private void Start() => wind = Wind._; 

    /// <summary>
    /// Enquanto "burning" é True, é aplicado a aceleração
    /// Aceleração Desejada: 10 kg-m/s => 98.1 N
    /// Massa do Foguete: 9.81 N = ≈ 1 KG
    /// Δt = ≈ 0.1s
    /// dT é multiplicador por 10 para a conversão de MS para Segundos, então é calculado a aceleração.  
    /// </summary>
    private void FixedUpdate() {
        if (burning) {
            rb.AddForce(10 * Time.deltaTime * rb.transform.up * 98 , ForceMode.Acceleration); // aplicação da aceleração 
            HeightParticles();
            
        } else if (smoothDrag) {
           rb.drag = .03f;
        }

        if (onAir) {
            Vector3 windDir = wind.WindForce(); 
            rb.AddForce(windDir); //adiciona o vento em cima do Rigidbody 
        }
    }
    
    /// <summary>
    /// O motor recebe o comando do RocketManager para iniciar a aplicação de força 
    /// </summary>
    /// <param name="t">Tempo de queima de combustivel, por quanto tempo vai acelerar.</param>
    public void Ignition(float t) { 
        burning = true;
        smoothDrag = true;
        onAir = true;
        rb.isKinematic = false;
        thrusterEmitter.SetActive(true); // ativa particula que representa a queima do combustivel.
        SoundManager._.PlaySequence(Sound.Ignition, transform); // inicia o som de ignição e queima.
        StartCoroutine(EngineCutoff(t));
    }

    /// <summary>
    /// Começa uma contagem para encerrar a aplicação de aceleração e desligamento do motor. 
    /// </summary>
    /// <param name="burnTime"> Após esse valor, o motor é desligado. </param>
    /// <returns></returns>
    IEnumerator EngineCutoff(float burnTime) {
        
        yield return new WaitForSeconds(burnTime);
        burning = false; // para de aplicar aceleração no foguete
        
        thrusterEmitter.SetActive(false); // encerramento das particulas
        SoundManager._.StopSequence(Sound.Ignition); // encerramento do som do motor
        
        yield return new WaitUntil(() => rb.velocity.y < 20); // aguarda a velocidade abaixar para continuar os procedimentos 

        rocket.EngineCutoff(engineId);
    }

    /// <summary>
    /// Faz a separação de um estagio motor do restante do foguete.
    /// </summary>
    public void StageSeparation() {
        
        transform.parent = null;
        Vector3 rocketVel = rb.velocity; // velocidade antiga

        var engines = Enum.GetValues(typeof(EngineID));
        if (engineId != (EngineID) engines.GetValue(engines.Length - 1)) { // se não for o ultimo estágio 
            SoundManager.PlaySound(Sound.Separation, rb.transform); // som de separação de estagio
            rb.AddForce(rb.transform.up * 1f,ForceMode.Impulse); // aplica uma força que representa a explosão que acontece para separação de estagios.
        }

        rb = gameObject.AddComponent<Rigidbody>(); // adiciona um rigidbody novo, para o estagio separado.
        rb.velocity = rocketVel; // e coloca a velocidade antiga, (aplicação de inércia)

        if (engineId == (EngineID) engines.GetValue(engines.Length - 1)) {
            UI._.Measure(rb); // se for o ultimo estagio o rigidbody das medidas é atualizado.
        }
    }
    
    public void PrepareLand() => parachute.LandManeuver(rb);
    
    /// <summary>
    /// <returns>retorna rigidbody do motor</returns>
    /// </summary>
    public Rigidbody GetRb => rb;
    
    /// <summary>
    /// <param name="smoothDrag"> Sofre valor do arrasto do ar </param>
    /// </summary>
    public bool SmoothDrag {
        set => smoothDrag = value;
    }

    /// <summary>
    /// Detecta se pousou com o chão.
    /// </summary>
    /// <param name="collision"></param>
    private void OnCollisionEnter(Collision collision) {
        if(!onAir) return;
        if (collision.collider.gameObject.layer == 6) {
            parachute.gameObject.SetActive(false);
            rb.angularDrag = 200;
        };
    }

    /// <summary>
    /// Calcula a altura e se estiver a menos de 10 metros do chão, ativa o efeito de poeira levantando do solo. 
    /// </summary>
    void HeightParticles() {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 15, Layer)) {
            if (hit.distance < 10 && !groundDust) {
                groundDust = true;
                var emissionModule = groundEmitter.GetComponent<ParticleSystem>().emission;
                emissionModule.rateOverTimeMultiplier = 100;    
            } else if (groundDust && hit.distance > 10) {
                var emissionModule = groundEmitter.GetComponent<ParticleSystem>().emission;
                emissionModule.rateOverTimeMultiplier = 0;
                groundDust = false;
            }
            
            if(!groundDust) return;            
            groundEmitter.transform.position = hit.point;
            groundEmitterEffector.transform.position = hit.point;
            groundEmitterEffector.gravity = Mathf.Lerp(0,-5,Mathf.InverseLerp(10,0, hit.distance));
        }
    }
}
