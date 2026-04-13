using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    float moveHorizontal;
    float moveVertical;
    public float velocidade = 5f; // Aumentei um pouco para ser mais perceptível

    public GameObject bulletPrefab;
    Rigidbody2D rb;
    public Vector2 bulletSpawnOffset = new Vector2(0, 1f);
    public float fireRate = 0.5f;
    private float nextFireTime = 0f;

    private Animator anim;

    private BulletPool pool;

    [SerializeField] private GameObject hitboxVisual;
    private SpriteRenderer hitboxRenderer;

    [SerializeField] private Color corA = Color.white;
    [SerializeField] private Color corB = Color.red;
    [SerializeField] private float velocidadeOscilacao = 5f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        pool = gameObject.AddComponent<BulletPool>();
        pool.bulletPrefab = bulletPrefab;

        pool.Initialize(50);

        hitboxRenderer = hitboxVisual.GetComponent<SpriteRenderer>();
        hitboxVisual.SetActive(false);
    }

    public void Moved(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        moveHorizontal = input.x;
        moveVertical = input.y;

        anim.SetBool("TurningLeft", moveHorizontal < -0.1f);
        anim.SetBool("TurningRight", moveHorizontal > 0.1f);
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        if (context.performed && nextFireTime >= fireRate)
        {
            GameObject bullet = pool.GetBullet();

            bullet.transform.position = transform.position + (Vector3)bulletSpawnOffset;

            BulletMovement bm = bullet.GetComponent<BulletMovement>();
            bm.SetPool(pool);
            bm.SetDirection(Vector2.up);
            bullet.GetComponent<BulletMovement>().SetDirection(Vector2.up);
            nextFireTime = 0;
        }
    }

    public void Focus(InputAction.CallbackContext context)
    {
        if (context.started || context.performed)
        {
            velocidade = 3f;
            hitboxVisual.SetActive(true);
        }
        else if (context.canceled)
        {
            velocidade = 5f;
            hitboxVisual.SetActive(false);
        }
    }

    void Update()
    {
        if (hitboxVisual.activeSelf)
        {
            float t = Mathf.PingPong(Time.time * velocidadeOscilacao, 1f);
            hitboxRenderer.color = Color.Lerp(corA, corB, t);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveHorizontal, moveVertical) * velocidade;

        if (nextFireTime < fireRate)
        {
            nextFireTime += Time.fixedDeltaTime;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyBullet"))
        {
            RestartScene();
        }
    }
    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}