using System;
using System.Collections;
using UnityEngine;

public class EnemyBasics : MonoBehaviour
{
    public float maxHealth = 3;
    private float currentHealth;
    public GameObject coletavel;
    public GameObject coletavelElite;
    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Vector3 baseScale;
    public float speedScale = 1;
    public float fireRateScale = 1;

    [SerializeField] private bool isElite = false;
    private bool isAcelerated = false;
    private bool isGiant = false;
    private bool isMutant = false;
    public bool IsMutant { get { return isMutant; } }
    public bool IsGiant { get { return isGiant; } }

    private static int resetCount = 0;
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
    [Range(0f, 1f)] public float volumeExplosao = 0.7f;
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

        // Aplica a cor base correta ao reativar no pool
        if (spriteRenderer != null)
        {
            StopMutantBlinkIfNeeded();
            spriteRenderer.color = baseColor;
        }
    }

    public void ResetEnemy()
    {
        resetCount++;

        // Reseta atributos base antes de aplicar a variação
        ResetBaseStats();

        if (resetCount % 100 == 0)
        {
            Giant();
            Debug.Log("Giant Enemy Spawned (special)");
            return;
        }

        int normalValue = UnityEngine.Random.Range(0, 100);
        if (normalValue < 90)
        {
            Normal();
            Debug.Log("Normal Enemy Spawned");
            return;
        }

        int randomValue = UnityEngine.Random.Range(0, 100);

        // 0..44 -> Accelerated (45%)
        // 45..89 -> Mutant (45%)
        // 90..99 -> Giant (10%)
        if (randomValue < 45)
        {
            Accelerated();
            Debug.Log("Accelerated Enemy Spawned");
        }
        else if (randomValue < 90)
        {
            Mutant();
            Debug.Log("Mutant Enemy Spawned");
        }
        else
        {
            Giant();
            Debug.Log("Giant Enemy Spawned");
        }
    }

    private void ResetBaseStats()
    {
        StopMutantBlinkIfNeeded();
        transform.localScale = baseScale; // Garante que volta ao tamanho normal
        baseColor = Color.white;
        speedScale = 1f;
        fireRateScale = 1f;
        minExplosao = baseMinExplosao;
        maxExplosao = baseMaxExplosao;
        offsetRange = baseOffsetRange;

        isGiant = false;
        isAcelerated = false;
        isMutant = false;
    }

    public void Normal()
    {
        ResetBaseStats();
        currentHealth = maxHealth;
        spriteRenderer.color = baseColor;
    }

    public void Giant()
    {
        ResetBaseStats();
        currentHealth = maxHealth * 2;

        // Ajustado: Mais explosões, porém com o offset proporcional ao tamanho do sprite (1.5x)
        minExplosao = 4;
        maxExplosao = 6;
        offsetRange = baseOffsetRange * 1.5f; // Mantém a explosão estritamente dentro da área do Gigante

        transform.localScale = baseScale * 1.5f;
        speedScale = 0.5f;

        isGiant = true;
        spriteRenderer.color = baseColor;
    }

    public void Accelerated()
    {
        ResetBaseStats();
        currentHealth = maxHealth;
        baseColor = new Color(1f, 0.92f, 0.45f, 1f); // Amarelado
        fireRateScale = 2f;
        speedScale = 1.5f;

        isAcelerated = true;
        spriteRenderer.color = baseColor;
    }

    public void Mutant()
    {
        ResetBaseStats();
        currentHealth = maxHealth;
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
            if (spriteRenderer != null) spriteRenderer.color = Color.white;
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
        if (collision.gameObject.CompareTag("Bullet"))
        {
            currentHealth -= collision.GetComponent<BulletMovement>().damage;

            Vector3 pontoDeContato = collision.ClosestPoint(transform.position);

            if (ImpactPool.Instance != null)
            {
                GameObject impact = ImpactPool.Instance.SpawnImpact(pontoDeContato - new Vector3(0, 0.2f, 0));
                impact.transform.SetParent(transform);
            }

            if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());

            Destroy(collision.gameObject);

            if (currentHealth <= 0) Morrer();
        }
    }

    void Morrer()
    {
        // Raio de espalhamento maior para gigantes
        float raioEspalhamento = isGiant ? 1.2f : 0.6f;

        // Lógica de Drops com espalhamento
        if (isElite)
        {
            if (coletavelElite != null)
                SpawnColetavelComEspalhamento(coletavelElite, 1, raioEspalhamento);

            if (isGiant && coletavel != null)
            {
                SpawnColetavelComEspalhamento(coletavel, 5, raioEspalhamento);
            }
        }
        else
        {
            int quantidade = isGiant ? 10 : 1;
            if (coletavel != null)
            {
                SpawnColetavelComEspalhamento(coletavel, quantidade, raioEspalhamento);
            }
        }

        if (CameraShakeController.Instance != null)
        {
            CameraShakeController.Instance.Shake(isGiant ? 0.5f : 0.2f);
        }

        OnDeathExplosion?.Invoke(transform.position, minExplosao, maxExplosao, offsetRange);

        if (somExplosao != null)
        {
            AudioSource.PlayClipAtPoint(somExplosao, Camera.main.transform.position, volumeExplosao);
        }

        // Achievements
        AchievementManager.Instance.AddProgress("enemy_kill", 1);
        if (isElite) AchievementManager.Instance.AddProgress("elite_enemy_kill", 1);
        if (isGiant) AchievementManager.Instance.AddProgress("giant_enemy_kill", 1);
        if (isElite && isGiant) AchievementManager.Instance.AddProgress("elite_giant_enemy_kill", 1);
        if (isAcelerated) AchievementManager.Instance.AddProgress("accelerated_enemy_kill", 1);
        if (isAcelerated && isElite) AchievementManager.Instance.AddProgress("accelerated_elite_enemy_kill", 1);
        if (isMutant) AchievementManager.Instance.AddProgress("mutant_enemy_kill", 1);
        if (isMutant && isElite) AchievementManager.Instance.AddProgress("mutant_elite_enemy_kill", 1);

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Instancia múltiplos coletáveis ao redor da posição do inimigo com um offset aleatório.
    /// </summary>
    private void SpawnColetavelComEspalhamento(GameObject prefab, int quantidade, float raio)
    {
        if (prefab == null) return;

        for (int i = 0; i < quantidade; i++)
        {
            // Se for apenas 1 item, spawna no centro. Se forem vários, sorteia uma posição no círculo
            Vector2 offset = quantidade > 1 ? UnityEngine.Random.insideUnitCircle * raio : Vector2.zero;
            Vector3 posDrop = transform.position + (Vector3)offset;

            Instantiate(prefab, posDrop, Quaternion.identity);
        }
    }
}