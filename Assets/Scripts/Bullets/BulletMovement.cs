using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    public float speed = 5f;
    public float TimeToDeath = 3f;

    private Vector2 direction;
    private Rigidbody2D rb;
    private float timer;

    private BulletPool pool;

    private Vector3 initialScale;
    [Tooltip("Fator aplicado às balas quando atiradas por inimigos gigantes")]
    public float giantBulletScale = 1.5f;
    private bool isGiantBullet = false;

    public float damage;

    void Awake()
    {
        initialScale = transform.localScale;
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 dir, float spd)
    {
        direction = dir.normalized;
        speed = spd;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetPool(BulletPool p)
    {
        pool = p;
    }

    // Configura se a bala é gigante e aplica a escala visual
    public void SetGiant(bool giant)
    {
        isGiantBullet = giant;
        if (isGiantBullet)
        {
            transform.localScale = initialScale * giantBulletScale;
        }
        else
        {
            transform.localScale = initialScale;
        }
    }

    void OnEnable()
    {
        timer = 0f;
        SetGiant(false);
    }

    void FixedUpdate()
    {
        // Se for gigante, reduz a velocidade em 25% (multiplica por 0.75)
        float multiplicadorVelocidadeTamanho = isGiantBullet ? 0.75f : 1f;

        // Anula a câmera lenta para balas do jogador
        float multiplicadorVelocidadeTempo = this.gameObject.CompareTag("EnemyBullet") ? 1f : 1f / Time.timeScale;

        rb.linearVelocity = direction * speed * multiplicadorVelocidadeTamanho * multiplicadorVelocidadeTempo;

        timer += Time.fixedDeltaTime;

        if (timer >= TimeToDeath)
        {
            pool.ReturnBullet(gameObject);
        }
    }
}