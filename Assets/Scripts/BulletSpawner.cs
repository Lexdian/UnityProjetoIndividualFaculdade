using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;

    [Header("Configuração do círculo")]
    public int directionsCount = 8;
    public float spawnDistance = 1f;

    [Header("Rajada")]
    public int bulletsPerBurst = 3;
    public float speedIncrement = 0.5f;
    public float angleSpread = 5f;

    [Header("Movimento")]
    public float baseSpeed = 5f;
    public float rotationSpeed = 30f;

    [Header("Disparo")]
    public float fireRate = 1f;

    [Header("Deadzone")]
    public float minYToShoot = 0f;

    [Header("Target")]
    public Transform target;

    [Header("Pool")]
    public float bulletLifetime = 3f;

    private float timer;
    private BulletPool pool;

    void Start()
    {
        // cria pool automaticamente
        pool = gameObject.AddComponent<BulletPool>();
        pool.bulletPrefab = bulletPrefab;

        int bulletsPerShot = directionsCount * bulletsPerBurst;
        int poolSize = 300;

        pool.Initialize(poolSize);
    }

    public void Setup(
    int directions,
    int burst,
    float rate,
    float rotation,
    float speed
)
    {
        directionsCount = directions;
        bulletsPerBurst = burst;
        fireRate = rate;
        rotationSpeed = rotation;
        baseSpeed = speed;
    }

    void Update()
    {
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        if (transform.position.y < minYToShoot)
            return;

        timer += Time.deltaTime;
        if (timer >= fireRate)
        {
            Shoot();
            timer = 0f;
        }
    }

    void Shoot()
    {
        float angleStep = 360f / directionsCount;

        Vector2 baseDirection = target != null
            ? (target.position - transform.position).normalized
            : Vector2.up;

        float baseAngleOffset = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

        for (int directionIndex = 0; directionIndex < directionsCount; directionIndex++)
        {
            float baseAngle = directionIndex * angleStep + baseAngleOffset + transform.eulerAngles.z;

            for (int burstIndex = 0; burstIndex < bulletsPerBurst; burstIndex++)
            {
                float angleOffset = (burstIndex - (bulletsPerBurst - 1) / 2f) * angleSpread;
                float finalAngle = baseAngle + angleOffset;

                float rad = finalAngle * Mathf.Deg2Rad;

                Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                Vector2 spawnPosition = (Vector2)transform.position + direction * spawnDistance;

                GameObject bullet = pool.GetBullet();

                bullet.transform.position = spawnPosition;

                float finalSpeed = baseSpeed + burstIndex * speedIncrement;

                BulletMovement bm = bullet.GetComponent<BulletMovement>();
                bm.SetPool(pool);
                bm.TimeToDeath = bulletLifetime;
                bm.SetDirection(direction, finalSpeed);
            }
        }
    }
    public void ResetFireTimer(float value)
    {
        timer = value;
    }
}