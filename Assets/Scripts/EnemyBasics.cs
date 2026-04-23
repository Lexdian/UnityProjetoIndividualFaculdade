using UnityEngine;

public class EnemyBasics : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;
    public GameObject coletavel;
    private Animator anim;

    [Header("Sons")]
    public AudioClip somExplosao;
    [Range(0f, 1f)] public float volumeExplosao = 0.7f; // Controle de volume direto aqui

    void OnEnable()
    {
        currentHealth = maxHealth;
    }

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            currentHealth--;

            if (anim != null) anim.SetTrigger("Damaged");

            // Se você usa Object Pooling para as balas do Player, 
            // troque Destroy por collision.gameObject.SetActive(false);
            Destroy(collision.gameObject);

            if (currentHealth <= 0) Morrer();
        }
    }

    void Morrer()
    {
        // 1. Drop de item
        if (coletavel != null)
        {
            Instantiate(coletavel, transform.position, Quaternion.identity);
        }

        // 2. Feedback Visual (Tremor)
        if (CameraShakeController.Instance != null)
        {
            CameraShakeController.Instance.Shake(0.2f);
        }

        // 3. Feedback Sonoro
        if (somExplosao != null)
        {
            // Usamos a posição da câmera para o som não ficar "longe" no 2D
            Vector3 somPos = Camera.main.transform.position;
            AudioSource.PlayClipAtPoint(somExplosao, somPos, volumeExplosao);
        }

        // 4. Desativa o inimigo (retorna para a pool)
        gameObject.SetActive(false);
    }
}