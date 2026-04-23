using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    [Header("Áudio")]
    public AudioClip somTiro;
    public AudioClip somAtivarEspecial;
    public AudioClip somGraze; // NOVO: Arraste o som de graze aqui
    [Range(0f, 1f)] public float volumeTiro = 0.5f;
    [Range(0f, 1f)] public float volumeEspecial = 0.8f;
    [Range(0f, 1f)] public float volumeGraze = 0.4f; // Volume menor por ser repetitivo
    private AudioSource audioSource;

    float moveHorizontal;
    float moveVertical;
    public float velocidade = 5f;

    [Header("Configurações de Tiro")]
    public GameObject bulletPrefab;
    Rigidbody2D rb;
    public Vector2 bulletSpawnOffset = new Vector2(0, 1f);
    public float fireRateBase = 0.5f;
    public float fireRateMinimo = 0.1f;
    private float nextFireTime = 0f;

    private Animator anim;
    private BulletPool pool;

    [SerializeField] private GameObject hitboxVisual;
    private SpriteRenderer hitboxRenderer;
    private Camera mainCam;

    [SerializeField] private Color corA = Color.white;
    [SerializeField] private Color corB = Color.red;
    [SerializeField] private float velocidadeOscilacao = 5f;

    private int score = 0;

    [Header("Sistema de Graze & Especial")]
    public float especialMaximo = 100f;
    public float especialAtual = 0f;
    public float ganhoGraze = 5f;
    public float perdaEspecialPorSegundo = 0.01f;
    public float tempoParaComecarPerder = 1.5f;
    private float timerUltimoGraze;

    [Header("UI do Especial")]
    [SerializeField] private Text textoEspecial;
    [SerializeField] private Text textoScore;

    [Header("Habilidade Especial (Time Slow)")]
    public float custoAtivacaoMinimo = 20f;
    public float taxaConsumoHabilidade = 15f;
    public float fatorCameraLenta = 0.5f;
    private bool estaUsandoHabilidade = false;

    void Start()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        pool = gameObject.AddComponent<BulletPool>();
        pool.bulletPrefab = bulletPrefab;
        pool.Initialize(50);

        hitboxRenderer = hitboxVisual.GetComponent<SpriteRenderer>();
        hitboxRenderer.enabled = false;
    }

    // ... (Métodos Moved, CalcularFireRate e Focus permanecem iguais)

    public void Moved(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveHorizontal = input.x;
        moveVertical = input.y;

        if (anim != null)
        {
            anim.SetBool("TurningLeft", moveHorizontal < -0.1f);
            anim.SetBool("TurningRight", moveHorizontal > 0.1f);
        }
    }

    float CalcularFireRate()
    {
        float percentualEspecial = especialAtual / especialMaximo;
        return Mathf.Lerp(fireRateBase, fireRateMinimo, percentualEspecial);
    }

    public void Focus(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            velocidade = 3f;
            hitboxRenderer.enabled = true;
        }
        else if (context.canceled)
        {
            velocidade = 5f;
            hitboxRenderer.enabled = false;
        }
    }

    void Update()
    {
        if (hitboxRenderer.enabled)
        {
            float t = Mathf.PingPong(Time.time * velocidadeOscilacao, 1f);
            hitboxRenderer.color = Color.Lerp(corA, corB, t);
        }

        float currentFireRate = CalcularFireRate();
        nextFireTime += Time.deltaTime;

        if (Keyboard.current.zKey.isPressed && nextFireTime >= currentFireRate)
        {
            Disparar();
            nextFireTime = 0;
        }

        AtualizarInterface();
        ControlarHabilidadeEspecial();
    }

    void ControlarHabilidadeEspecial()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && especialAtual >= custoAtivacaoMinimo && !estaUsandoHabilidade)
        {
            if (somAtivarEspecial != null && audioSource != null)
            {
                audioSource.PlayOneShot(somAtivarEspecial, volumeEspecial);
            }

            AtivarHabilidade(true);
        }

        if (estaUsandoHabilidade)
        {
            especialAtual -= taxaConsumoHabilidade * Time.unscaledDeltaTime;

            if (especialAtual <= 0)
            {
                especialAtual = 0;
                AtivarHabilidade(false);
            }
        }
        else
        {
            if (especialAtual > 0 && Time.time > timerUltimoGraze + tempoParaComecarPerder)
            {
                especialAtual -= perdaEspecialPorSegundo * Time.deltaTime;
            }
        }
    }

    void AtivarHabilidade(bool ativar)
    {
        estaUsandoHabilidade = ativar;

        if (ativar)
        {
            Time.timeScale = fatorCameraLenta;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
        }
        else
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }

    void AtualizarInterface()
    {
        if (textoEspecial != null)
        {
            textoEspecial.text = "Adrenalina: " + especialAtual.ToString("F0") + "%";
            textoEspecial.color = (especialAtual >= especialMaximo) ? Color.yellow : Color.white;
        }

        if (textoScore != null)
        {
            textoScore.text = "Score: " + score.ToString("F0");
        }
    }

    public void GanharPonstos(int pontos)
    {
        score += pontos * (int)especialAtual;
    }

    void Disparar()
    {
        GameObject bullet = pool.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = transform.position + (Vector3)bulletSpawnOffset;
            BulletMovement bm = bullet.GetComponent<BulletMovement>();
            bm.SetPool(pool);
            bm.SetDirection(Vector2.up);

            if (somTiro != null && audioSource != null)
            {
                audioSource.PlayOneShot(somTiro, volumeTiro);
            }
        }
    }

    void FixedUpdate()
    {
        float multiplicadorVelocidadeTempo = estaUsandoHabilidade ? (1f / fatorCameraLenta) : 1f;
        rb.linearVelocity = new Vector2(moveHorizontal, moveVertical) * velocidade * multiplicadorVelocidadeTempo;
        RestringirAoViewport();
    }

    void RestringirAoViewport()
    {
        Vector3 viewPos = mainCam.WorldToViewportPoint(rb.position);
        viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
        viewPos.y = Mathf.Clamp(viewPos.y, 0.05f, 0.95f);
        rb.position = mainCam.ViewportToWorldPoint(viewPos);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyBullet"))
        {
            GameOverScene();
        }
    }

    void GameOverScene()
    {
        SceneManager.LoadScene("GameOver");
    }

    public void RegistrarGraze()
    {
        if (estaUsandoHabilidade) return;

        // TOCA O SOM DO GRAZE
        if (somGraze != null && audioSource != null)
        {
            // Usamos PlayOneShot para que sons de múltiplos projéteis se acumulem
            audioSource.PlayOneShot(somGraze, volumeGraze);
        }

        especialAtual += ganhoGraze;
        especialAtual = Mathf.Clamp(especialAtual, 0, especialMaximo);
        timerUltimoGraze = Time.time;
    }
}