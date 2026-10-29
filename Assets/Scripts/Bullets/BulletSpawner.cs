using System;
using UnityEngine;

public class BulletSpawner : MonoBehaviour
{
    public float bulletLifetime = 3f;
    public Transform target;
    public float minYToShoot = -2f;

    [Header("Configurações de áudio")]
    public AudioClip somTiroInimigo;
    [Range(0f, 1f)] public float volumeTiro = 0.4f;
    private AudioSource audioSource;

    [Header("Escalonamento para Balas de Parar (Stop Pattern)")]
    [Tooltip("Ativa o aumento progressivo do alcance antes da 1ª parada")]
    public bool usarEscalonamentoStop = true;
    [Tooltip("Tempo de caminhada da 1ª rajada (anda pouco)")]
    public float tempoCaminhadaInicial = 0.2f;
    [Tooltip("Quanto tempo é somado a cada nova rajada no disparo (ex: 0.2s -> +0.2s = 0.4s)")]
    public float incrementoTempoPorRajada = 0.2f;
    [Tooltip("Tempo padrão de caminhada nos ciclos seguintes")]
    public float tempoAndandoPadrao = 0.8f;
    [Tooltip("Tempo que a bala fica travada no ar em cada parada")]
    public float tempoParadoPadrao = 0.4f;

    private float timer;
    private BulletPool pool;

    // Variáveis do Pattern
    private GameObject bulletPrefab;
    private bool aimAtPlayer;
    private int directionsCount;
    private int bulletsPerBurst;
    private float fireRate;
    private float rotationSpeed;
    private float baseSpeed;
    private float angleStep;
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

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        if (target == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }

        AtualizarBulletPool();
    }

    public void ApplyPattern(ShootingPattern p)
    {
        bulletPrefab = p.bullet;
        aimAtPlayer = p.aimAtPlayer;
        directionsCount = p.directions;
        bulletsPerBurst = p.burst;
        fireRate = p.fireRate;
        rotationSpeed = p.rotationSpeed;
        baseSpeed = p.bulletSpeed;
        angleStep = p.angleStep;
        angleSpread = p.angleSpread;
        speedIncrement = p.speedIncrement;
        shotsPerVolley = p.shotsPerVolley;
        timeBetweenVolleys = p.timeBetweenVolleys;

        usarEscalonamentoStop = p.usarEscalonamentoStop;
        tempoCaminhadaInicial = p.tempoCaminhadaInicial;
        incrementoTempoPorRajada = p.incrementoTempoPorRajada;
        tempoAndandoPadrao = p.tempoAndandoPadrao;
        tempoParadoPadrao = p.tempoParadoPadrao;

        if (angleStep == 0f)
        {
            angleStep = (directionsCount > 1) ? 360f / directionsCount : 0f;
        }

        AtualizarBulletPool();
    }

    private void AtualizarBulletPool()
    {
        if (bulletPrefab == null) return;

        if (pool == null)
        {
            pool = GetComponent<BulletPool>();
            if (pool == null) pool = gameObject.AddComponent<BulletPool>();
        }

        if (pool.bulletPrefab != bulletPrefab)
        {
            pool.bulletPrefab = bulletPrefab;
            pool.Initialize(100);
        }
    }

    void Update()
    {
        if (transform.position.y < minYToShoot) return;

        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);

        // Obtém o multiplicador de cadência de tiro do inimigo (Acelerado atira mais rápido)
        EnemyBasics eb = GetComponentInParent<EnemyBasics>();
        float fireRateMult = (eb != null) ? eb.fireRateScale : 1f;

        timer += Time.deltaTime;
        float baseWaitTime = isResting ? timeBetweenVolleys : fireRate;
        float currentWaitTime = baseWaitTime / fireRateMult; // Reduz o tempo de espera conforme o fireRateScale aumenta

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
        if (pool == null || bulletPrefab == null) return;

        if (somTiroInimigo != null && audioSource != null)
        {
            audioSource.PlayOneShot(somTiroInimigo, volumeTiro);
        }

        bool shooterIsGiant = false;
        float bulletSpeedScale = 1f;

        EnemyBasics eb = GetComponentInParent<EnemyBasics>();
        if (eb != null)
        {
            shooterIsGiant = eb.IsGiant;
            bulletSpeedScale = eb.bulletSpeedScale; // Coleta a escala de velocidade do projétil do inimigo
        }

        float tempoPrimeiraCaminhadaCalculado = tempoCaminhadaInicial + (currentVolleyCount * incrementoTempoPorRajada);

        float baseAngle = -90f;

        if (aimAtPlayer && target != null)
        {
            Vector2 dirToTarget = (target.position - transform.position).normalized;
            baseAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;
        }

        for (int i = 0; i < directionsCount; i++)
        {
            float currentDirectionAngle = baseAngle - (directionsCount - 1) * angleStep / 2f + (i * angleStep) + transform.eulerAngles.z;

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
                    if (bm != null)
                    {
                        bm.SetPool(pool);
                        bm.TimeToDeath = bulletLifetime;

                        float finalSpeed = baseSpeed + (b * speedIncrement);
                        bm.SetDirection(moveDir, finalSpeed);

                        // Aplica o multiplicador de velocidade da bala do inimigo (ex: Acelerado)
                        bm.SetSpeedMultiplier(bulletSpeedScale);

                        if (shooterIsGiant)
                        {
                            bm.SetGiant(true);
                        }
                    }

                    if (usarEscalonamentoStop)
                    {
                        BulletMovementStop bStop = bullet.GetComponent<BulletMovementStop>();
                        if (bStop != null)
                        {
                            bStop.ConfigurarEscalonamento(tempoPrimeiraCaminhadaCalculado, tempoAndandoPadrao, tempoParadoPadrao);
                        }
                    }
                }
            }
        }
    }
}