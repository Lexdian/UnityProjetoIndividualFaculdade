using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    public float speed = 5f;
    public float TimeToDeath = 3f;

    private Vector2 direction;
    private Rigidbody2D rb;
    private float timer;

    private BulletPool pool;

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

    void OnEnable()
    {
        timer = 0f;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;

        timer += Time.fixedDeltaTime;

        if (timer >= TimeToDeath)
        {
            pool.ReturnBullet(gameObject);
        }
    }
}