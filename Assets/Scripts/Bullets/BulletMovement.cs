using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    public float speed = 5f;
    public float TimeToDeath = 3f;

    private Vector2 direction;
    protected Rigidbody2D rb;
    private float timer;

    private BulletPool pool;

    private Vector3 initialScale;
    [Tooltip("Fator aplicado às balas quando atiradas por inimigos gigantes")]
    public float giantBulletScale = 2.2f;
    private bool isGiantBullet = false;

    // Propriedade pública para consultar se esta bala é gigante
    public bool IsGiant => isGiantBullet;

    private float speedMultiplier = 1f;

    public float damage;

    protected virtual void Awake()
    {
        initialScale = transform.localScale;
        GarantirRigidbody();
    }

    private void GarantirRigidbody()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
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

    public void SetSpeedMultiplier(float mult)
    {
        speedMultiplier = mult;
    }

    protected virtual void OnEnable()
    {
        timer = 0f;
        speedMultiplier = 1f;
        SetGiant(false);
        GarantirRigidbody();
    }

    void FixedUpdate()
    {
        float multiplicadorVelocidadeTamanho = isGiantBullet ? 0.75f : 1f;
        float multiplicadorVelocidadeTempo = this.gameObject.CompareTag("EnemyBullet") ? 1f : 1f / Time.timeScale;

        Move(multiplicadorVelocidadeTamanho, multiplicadorVelocidadeTempo);

        timer += Time.fixedDeltaTime;

        if (timer >= TimeToDeath && pool != null)
        {
            pool.ReturnBullet(gameObject);
        }
    }

    protected virtual void Move(float multTam, float multTemp)
    {
        GarantirRigidbody();

        if (rb != null)
        {
            rb.linearVelocity = direction * speed * speedMultiplier * multTam * multTemp;
        }
    }
}