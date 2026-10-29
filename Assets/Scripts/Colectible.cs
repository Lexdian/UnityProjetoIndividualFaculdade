using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Configurações de Queda")]
    public float fallSpeed = 3f;
    public int scoreValue = 100;

    [Header("Efeito de Salto Inicial (Pop-Up)")]
    [Tooltip("Força do pulo inicial para cima ao spawnar")]
    public float initialJumpForce = 4.5f;

    [Tooltip("Gravidade aplicada para desacelerar o pulo e iniciar a queda")]
    public float gravity = 12f;

    private float currentVerticalSpeed;

    [Header("Áudio")]
    public AudioClip somColeta;
    [Range(0f, 1f)] public float volumeColeta = 0.6f;

    protected Camera mainCam;

    // --- VARIÁVEIS DE ATRAÇÃO ---
    public bool podeSerAtraido = true;
    protected bool atraido = false;
    protected Transform alvoAtracao;
    protected float velocidadeAtracao = 15f;
    protected bool foiAtraidoPorPoc = false;

    protected virtual void Start()
    {
        mainCam = Camera.main;
    }

    protected virtual void OnEnable()
    {
        // Define a velocidade inicial do pulo para suportar instanciação e Object Pooling
        currentVerticalSpeed = initialJumpForce;

        if (PlayerMovement.Instance != null && podeSerAtraido)
        {
            PlayerMovement.Instance.RegistrarColetavel(this);
        }
    }

    protected virtual void OnDisable()
    {
        if (PlayerMovement.Instance != null && podeSerAtraido)
        {
            PlayerMovement.Instance.DesregistrarColetavel(this);
        }
    }

    protected virtual void Update()
    {
        // Se foi ativado o modo de atração (ex: POC no topo da tela), move-se direto para o jogador
        if (atraido && alvoAtracao != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, alvoAtracao.position, velocidadeAtracao * Time.deltaTime);
        }
        else
        {
            // 1. Aplica a gravidade para reduzir a velocidade de subida até começar a cair
            currentVerticalSpeed -= gravity * Time.deltaTime;

            // 2. Limita a velocidade máxima de queda para não ultrapassar o fallSpeed
            if (currentVerticalSpeed < -fallSpeed)
            {
                currentVerticalSpeed = -fallSpeed;
            }

            // 3. Move o objeto verticalmente
            transform.Translate(Vector2.up * currentVerticalSpeed * Time.deltaTime);

            // 4. Verifica se saiu do limite inferior da tela
            VerificarLimiteInferior();
        }
    }

    public bool EstaSendoAtraido() => atraido;

    public void AtrairPara(Transform alvo, float velocidade, bool eComboPoc = true)
    {
        atraido = true;
        alvoAtracao = alvo;
        velocidadeAtracao = velocidade;
        foiAtraidoPorPoc = eComboPoc;
    }

    protected void VerificarLimiteInferior()
    {
        if (mainCam == null) return;

        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);

        if (viewPos.y < -0.1f)
        {
            // --- NOTIFICA O SISTEMA DE PERÍCIA QUE O ITEM FOI PERDIDO ---
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.RegistrarColetavelPerdido();
            }

            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (somColeta != null && mainCam != null)
            {
                AudioSource.PlayClipAtPoint(somColeta, mainCam.transform.position, volumeColeta);
            }

            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                if (foiAtraidoPorPoc)
                {
                    player.ColetarItemPoc(scoreValue);
                }
                else if (scoreValue > 0)
                {
                    player.GanharPontos(scoreValue);
                }
            }

            Coletar();
        }
    }

    public virtual void Coletar()
    {
        Debug.Log("Item coletado! + " + scoreValue);
        Destroy(gameObject);
    }
}