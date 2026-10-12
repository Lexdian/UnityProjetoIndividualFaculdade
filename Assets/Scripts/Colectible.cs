using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Configurações")]
    public float fallSpeed = 3f;
    public int scoreValue = 100;

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
        // Se foi ativado o modo de atração, move-se em direção ao jogador
        if (atraido && alvoAtracao != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, alvoAtracao.position, velocidadeAtracao * Time.deltaTime);
        }
        else
        {
            // 1. Faz o item cair constantemente
            transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

            // 2. Verifica se saiu da visão da câmera
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
            Destroy(gameObject);
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Toca o som de coleta
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