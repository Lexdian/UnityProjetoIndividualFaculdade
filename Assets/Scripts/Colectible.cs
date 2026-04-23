using UnityEngine;

public class Collectible : MonoBehaviour
{
    [Header("Configurações")]
    public float fallSpeed = 3f;
    public int scoreValue = 100;

    [Header("Áudio")]
    public AudioClip somColeta;
    [Range(0f, 1f)] public float volumeColeta = 0.6f;

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
    }

    void Update()
    {
        // 1. Faz o item cair constantemente
        transform.Translate(Vector2.down * fallSpeed * Time.deltaTime);

        // 2. Verifica se saiu da visão da câmera
        VerificarLimiteInferior();
    }

    void VerificarLimiteInferior()
    {
        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);

        if (viewPos.y < -0.1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Toca o som antes de destruir o objeto
            if (somColeta != null)
            {
                // Usamos a posição da câmera para o som ser ouvido perfeitamente
                AudioSource.PlayClipAtPoint(somColeta, mainCam.transform.position, volumeColeta);
            }

            collision.GetComponent<PlayerMovement>().GanharPonstos(scoreValue);
            Coletar();
        }
    }

    void Coletar()
    {
        Debug.Log("Item coletado! + " + scoreValue);
        Destroy(gameObject);
    }
}