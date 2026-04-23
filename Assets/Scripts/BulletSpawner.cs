using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public GameObject bulletPrefab;
    public float bulletLifetime = 3f;
    public Transform target;
    public float minYToShoot = -2f;

    [Header("Configurações de Áudio")]
    public AudioClip somTiroInimigo;
    [Range(0f, 1f)] public float volumeTiro = 0.4f;
    private AudioSource audioSource;

    private float timer;
    private BulletPool pool;

    // Variáveis do Pattern
    private bool aimAtPlayer;
    private int directionsCount;
    private int bulletsPerBurst;
    private float fireRate;
    private float rotationSpeed;
    private float baseSpeed;
    private float angleSpread;
    private float speedIncrement;
    private int shotsPerVolley;
    private float timeBetweenVolleys;

    private int currentVolleyCount = 0;
    private bool isResting = false;

    void OnEnable()
    {
        timer = 0;
        currentVolleyCount = 0;
        isResting = false;

        // Inicializa Áudio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (pool == null)
        {
            pool = gameObject.AddComponent<BulletPool>();
            pool.bulletPrefab = bulletPrefab;
            pool.Initialize(100);
        }

        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    public void ApplyPattern(ShootingPattern p)
    {
        aimAtPlayer = p.aimAtPlayer;
        directionsCount = p.directions;
        bulletsPerBurst = p.burst;
        fireRate = p.fireRate;
        rotationSpeed = p.rotationSpeed;
        baseSpeed = p.bulletSpeed;
        angleSpread = p.angleSpread;
        speedIncrement = p.speedIncrement;
        shotsPerVolley = p.shotsPerVolley;
        timeBetweenVolleys = p.timeBetweenVolleys;
    }

    void Update()
    {
        if (transform.position.y < minYToShoot) return;

        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        timer += Time.deltaTime;
        float currentWaitTime = isResting ? timeBetweenVolleys : fireRate;

        if (timer >= currentWaitTime)
        {
            if (isResting) isResting = false;
            else
            {
                Shoot();
                currentVolleyCount++;
                if (currentVolleyCount >= shotsPerVolley)
                {
                    isResting = true;
                    currentVolleyCount = 0;
                }
            }
            timer = 0f;
        }
    }

    void Shoot()
    {
        // Toca o som uma vez por "disparo" (independente de quantas direções saem)
        // Isso evita que o som fique "estourado" em disparos circulares
        if (somTiroInimigo != null && audioSource != null)
        {
            audioSource.PlayOneShot(somTiroInimigo, volumeTiro);
        }

        float angleStep = (directionsCount > 1) ? 360f / directionsCount : 0f;
        float baseAngle = -90f;

        if (aimAtPlayer && target != null)
        {
            Vector2 dirToTarget = (target.position - transform.position).normalized;
            baseAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        }

        for (int i = 0; i < directionsCount; i++)
        {
            float currentDirectionAngle = baseAngle + (i * angleStep) + transform.eulerAngles.z;

            for (int b = 0; b < bulletsPerBurst; b++)
            {
                float offset = (b - (bulletsPerBurst - 1) / 2f) * angleSpread;
                float finalAngle = currentDirectionAngle + offset;

                float rad = finalAngle * Mathf.Deg2Rad;
                Vector2 moveDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));

                GameObject bullet = pool.GetBullet();
                if (bullet != null)
                {
                    bullet.transform.position = transform.position;
                    BulletMovement bm = bullet.GetComponent<BulletMovement>();
                    bm.SetPool(pool);
                    bm.TimeToDeath = bulletLifetime;

                    float finalSpeed = baseSpeed + (b * speedIncrement);
                    bm.SetDirection(moveDir, finalSpeed);
                }
            }
        }
    }
}