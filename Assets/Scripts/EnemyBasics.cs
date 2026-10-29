using System;
using System.Collections;
using UnityEngine;

public class EnemyBasics : MonoBehaviour
{
    public float maxHealth = 3;
    public float currentHealth;
    public GameObject coletavel;
    public GameObject coletavelElite;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;

    [Header("Multiplicadores de Status")]
    public float speedScale = 1f;
    public float fireRateScale = 1f;
    public float bulletSpeedScale = 1f;

    [SerializeField] private bool isElite = false;
    private bool isAcelerated = false;
    private bool isGiant = false;
    private bool isMutant = false;
    public bool IsMutant { get { return isMutant; } }
    public bool IsGiant { get { return isGiant; } }
    public bool IsAcelerated { get { return isAcelerated; } }

    private static int spawnsDesdeUltimoGigante = 0;
    private Color baseColor = Color.white;

    [Header("Configurações da Explosão")]
    int baseMinExplosao = 1;
    int baseMaxExplosao = 2;
    float baseOffsetRange = 0.5f;

    private int minExplosao;
    private int maxExplosao;
    private float offsetRange;

    [Header("Sons")]
    public AudioClip somExplosao;
    public AudioClip somImpacto;
    [Range(0f, 1f)] public float volumeExplosao = 0.7f;
    [Range(0f, 1f)] public float volumeImpacto = 0.7f;
    public delegate void OnDeathExplosionDelegate(Vector2 position, int minExplosao, int maxExplosao, float offsetRange);
    public OnDeathExplosionDelegate OnDeathExplosion;

    private Coroutine mutantBlinkCoroutine;
    private Coroutine damageFlashCoroutine;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    void OnEnable()
    {
        currentHealth = maxHealth;

        if (anim != null)
        {
            anim.ResetTrigger("Damaged");
            anim.Play("IdleEnemy", 0, 0f);
        }
    }

    private void OnDisable()
    {
        // Garante a limpeza total de modificadores antes do objeto voltar ao pool
        ResetBaseStats();
    }

    public void Normal()
    {
        spawnsDesdeUltimoGigante++;
        ResetBaseStats();
    }

    public void ResetEnemy()
    {
        Normal();

        bool forçaGigante = spawnsDesdeUltimoGigante >= 100;
        float fatorPericia = (EnemySpawner.Instance != null) ? EnemySpawner.Instance.PericiaNormalizada : 0f;

        float chanceAcelerado = 0.05f + (fatorPericia * 0.05f);
        float chanceGigante = 0.005f + (fatorPericia * 0.005f);
        float chanceMutante = 0.001f + (fatorPericia * 0.002f);

        bool rollAcelerado = UnityEngine.Random.value < chanceAcelerado;
        bool rollGigante = forçaGigante || (UnityEngine.Random.value < chanceGigante);
        bool rollMutante = UnityEngine.Random.value < chanceMutante;

        if (rollGigante)
        {
            spawnsDesdeUltimoGigante = 0;
            ApplyGiant();
        }

        if (rollAcelerado)
        {
            ApplyAccelerated();
        }

        if (rollMutante)
        {
            ApplyMutant();
        }

        if (!isMutant && spriteRenderer != null)
        {
            spriteRenderer.color = baseColor;
        }
    }

    private void ResetBaseStats()
    {
        StopMutantBlinkIfNeeded();
        if (damageFlashCoroutine != null)
        {
            StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = null;
        }

        isGiant = false;
        isAcelerated = false;
        isMutant = false;

        transform.localScale = baseScale;
        baseColor = Color.white;
        speedScale = 1f;
        fireRateScale = 1f;
        bulletSpeedScale = 1f;
        currentHealth = maxHealth;

        minExplosao = baseMinExplosao;
        maxExplosao = baseMaxExplosao;
        offsetRange = baseOffsetRange;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }

        GhostEffect ghost = GetComponent<GhostEffect>();
        if (ghost != null) ghost.isGhostActive = false;
    }

    private void ApplyGiant()
    {
        isGiant = true;

        float multiplicadorVida = 50f - Mathf.Clamp(maxHealth - 3f, 0f, 12f) * 1.25f;
        currentHealth = maxHealth * multiplicadorVida;

        minExplosao = 6;
        maxExplosao = 10;
        offsetRange = baseOffsetRange * 3f;

        transform.localScale = baseScale * 3f;
        speedScale *= 0.15f;
    }

    private void ApplyAccelerated()
    {
        isAcelerated = true;
        baseColor = new Color(1f, 0.92f, 0.45f, 1f);

        speedScale *= 2.0f;
        fireRateScale *= 2.5f;
        bulletSpeedScale *= 1.5f;

        GhostEffect ghost = GetComponent<GhostEffect>();
        if (ghost != null) ghost.isGhostActive = true;
    }

    private void ApplyMutant()
    {
        isMutant = true;
        mutantBlinkCoroutine = StartCoroutine(BlinkGreen());
    }

    private IEnumerator BlinkGreen()
    {
        Color green = new Color(0.6f, 1f, 0.6f, 1f);
        while (true)
        {
            if (spriteRenderer != null) spriteRenderer.color = green;
            yield return new WaitForSeconds(0.25f);
            if (spriteRenderer != null) spriteRenderer.color = baseColor;
            yield return new WaitForSeconds(0.25f);
        }
    }

    public IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = baseColor;
        }
    }

    private void StopMutantBlinkIfNeeded()
    {
        if (mutantBlinkCoroutine != null)
        {
            StopCoroutine(mutantBlinkCoroutine);
            mutantBlinkCoroutine = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!gameObject.activeInHierarchy) return;

        if (collision.gameObject.CompareTag("Bullet"))
        {
            BulletMovement bm = collision.GetComponent<BulletMovement>();
            if (bm != null)
            {
                currentHealth -= bm.damage;
            }

            if (somImpacto != null)
            {
                AudioSource.PlayClipAtPoint(somImpacto, Camera.main.transform.position, volumeImpacto);
            }

            Vector3 pontoDeContato = collision.ClosestPoint(transform.position);

            if (ImpactPool.Instance != null)
            {
                ImpactPool.Instance.SpawnImpact(pontoDeContato - new Vector3(0, 0.2f, 0));
            }

            Destroy(collision.gameObject);

            if (currentHealth <= 0)
            {
                Morrer();
            }
            else
            {
                if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
                damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());
            }
        }
    }

    void Morrer()
    {
        if (isGiant)
        {
            if (EnemySpawner.Instance != null)
            {
                EnemySpawner.Instance.NotificarMorteGigante();
            }

            GameObject[] balasInimigas = GameObject.FindGameObjectsWithTag("EnemyBullet");
            foreach (GameObject bala in balasInimigas)
            {
                bala.SetActive(false);
            }
        }

        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.RegistrarInimigoDerrotado();
        }

        float raioEspalhamento = isGiant ? 2.0f : 0.6f;

        if (isElite)
        {
            if (coletavelElite != null)
                SpawnColetavelComEspalhamento(coletavelElite, 1, raioEspalhamento);

            if (isGiant && coletavel != null)
            {
                SpawnColetavelComEspalhamento(coletavel, 10, raioEspalhamento);
            }
        }
        else
        {
            int quantidade = isGiant ? 25 : 1;
            if (coletavel != null)
            {
                SpawnColetavelComEspalhamento(coletavel, quantidade, raioEspalhamento);
            }
        }

        if (CameraShakeController.Instance != null)
        {
            CameraShakeController.Instance.Shake(isGiant ? 1.5f : 0.2f);
        }

        OnDeathExplosion?.Invoke(transform.position, minExplosao, maxExplosao, offsetRange);

        if (somExplosao != null)
        {
            AudioSource.PlayClipAtPoint(somExplosao, Camera.main.transform.position, volumeExplosao);
        }

        AchievementManager.Instance.AddProgress("enemy_kill", 1);
        if (isElite) AchievementManager.Instance.AddProgress("elite_enemy_kill", 1);

        if (isGiant)
        {
            AchievementManager.Instance.AddProgress("giant_enemy_kill", 1);
            if (isElite) AchievementManager.Instance.AddProgress("elite_giant_enemy_kill", 1);
        }

        if (isAcelerated)
        {
            AchievementManager.Instance.AddProgress("accelerated_enemy_kill", 1);
            if (isElite) AchievementManager.Instance.AddProgress("accelerated_elite_enemy_kill", 1);
        }

        if (isMutant)
        {
            AchievementManager.Instance.AddProgress("mutant_enemy_kill", 1);
            if (isElite) AchievementManager.Instance.AddProgress("mutant_elite_enemy_kill", 1);
        }

        if (isGiant && isAcelerated && !isMutant)
        {
            AchievementManager.Instance.AddProgress("giant_accelerated_enemy_kill", 1);
            if (isElite) AchievementManager.Instance.AddProgress("giant_accelerated_elite_enemy_kill", 1);
        }

        if (isGiant && isMutant && !isAcelerated)
        {
            AchievementManager.Instance.AddProgress("giant_mutant_enemy_kill", 1);
            if (isElite) AchievementManager.Instance.AddProgress("giant_mutant_elite_enemy_kill", 1);
        }

        if (isAcelerated && isMutant && !isGiant)
        {
            AchievementManager.Instance.AddProgress("accelerated_mutant_enemy_kill", 1);
            if (isElite) AchievementManager.Instance.AddProgress("accelerated_mutant_elite_enemy_kill", 1);
        }

        if (isGiant && isAcelerated && isMutant)
        {
            AchievementManager.Instance.AddProgress("triple_threat_enemy_kill", 1);
            if (isElite) AchievementManager.Instance.AddProgress("triple_threat_elite_enemy_kill", 1);
        }

        gameObject.SetActive(false);
    }

    private void SpawnColetavelComEspalhamento(GameObject prefab, int quantidade, float raio)
    {
        if (prefab == null) return;

        for (int i = 0; i < quantidade; i++)
        {
            Vector2 offset = quantidade > 1 ? UnityEngine.Random.insideUnitCircle * raio : Vector2.zero;
            Vector3 posDrop = transform.position + (Vector3)offset;

            Instantiate(prefab, posDrop, Quaternion.identity);
        }
    }
}