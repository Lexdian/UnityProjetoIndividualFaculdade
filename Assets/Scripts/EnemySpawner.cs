using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [System.Serializable]
    public class EnemyType
    {
        public string name;
        public GameObject prefab;
    }

    [Header("Configurações de Spawn")]
    public EnemyType[] enemies;
    public EnemyType[] eliteEnemies;
    private int spawnCount = 0;

    [Header("Limites de Borda da Tela")]
    [Tooltip("Margem segura do Viewport para garantir que sprites/colliders não fiquem fora da tela (0.12 = 12% da borda)")]
    [Range(0.05f, 0.25f)] public float margemBordaViewport = 0.12f;

    [Header("Intervalos de Spawn por Dificuldade (Segundos)")]
    [Tooltip("Tempo em segundos de espera antes de surgir o primeiro inimigo na fase")]
    public float atrasoPrimeiroSpawn = 2.0f;
    public float spawnRateNivel1 = 2.2f; // Fácil
    public float spawnRateNivel2 = 1.6f; // Médio
    public float spawnRateNivel3 = 1.0f; // Difícil

    [Header("Configuração do Inimigo Gigante")]
    [Tooltip("Tempo de espera extra (em segundos) adicionado ao próximo spawn quando surge um Gigante")]
    public float giantSpawnDelay = 4.0f;
    [Tooltip("Posição Y no mundo onde o Gigante irá surgir (padrão dos normais é 6f)")]
    public float posicaoYGigante = 7.0f;

    [Header("Grupo de Inimigos Normais")]
    [Range(0f, 1f)] public float chanceNormalGroupBase = 0.20f;
    [Tooltip("Aumento da chance por nível de dificuldade")]
    public float chanceNormalGroupPorNivel = 0.10f;
    [Tooltip("Quantidade MÁXIMA total de inimigos em um grupo (ex: 5 = 1 original + até 4 extras)")]
    public int maxInimigosNormaisGrupo = 5;
    [Tooltip("Intervalo em segundos entre cada spawn do grupo")]
    public float intervaloSpawnGrupoNormal = 0.15f;

    [Header("Grupo de Inimigos Elites (Apenas Dificuldade 3)")]
    [Range(0f, 1f)] public float chanceEliteGroupBase = 0.15f;
    [Tooltip("Quantidade MÁXIMA total de elites no grupo (ex: 3 = 1 original + até 2 extras)")]
    public int maxInimigosElitesGrupo = 3;
    [Tooltip("Intervalo em segundos entre cada spawn do grupo de elites")]
    public float intervaloSpawnGrupoElite = 0.25f;

    [Header("Proximidade do Spawn em Grupo")]
    [Tooltip("Distância máxima no eixo X em relação ao último inimigo spawnado no grupo")]
    public float dispersaoGrupoX = 1.5f;

    [Header("Controle de Dificuldade (Tempo)")]
    [Tooltip("Tempo em segundos para os inimigos passarem para o Nível 2 (Médio)")]
    public float tempoNivelMedio = 20f;

    [Tooltip("Tempo em segundos para os inimigos passarem para o Nível 3 (Difícil)")]
    public float tempoNivelDificil = 40f;

    [Header("Métrica de Perícia (Skill) do Jogador")]
    [SerializeField] private float periciaJogador = 0f;
    public float minPericia = 0f;
    public float maxPericia = 100f;

    [Header("Valores de Ajuste de Perícia")]
    public float ganhoPorInimigo = 1f;
    public float ganhoPorGraze = 2f;
    public float perdaPorMorte = 15f;
    public float perdaPorColetavelPerdido = 5f;

    public float PericiaAtual => periciaJogador;
    public float PericiaNormalizada => Mathf.Clamp01(periciaJogador / maxPericia);

    [Header("Trilha Sonora da Fase (MusicManager)")]
    [Tooltip("Música padrão da fase - Intro (opcional) e Loop")]
    [SerializeField] private AudioClip introMusicaFaseNormal;
    [SerializeField] private AudioClip loopMusicaFaseNormal;

    [Space(5)]
    [Tooltip("Música do Gigante Padrão - Intro (opcional) e Loop")]
    [SerializeField] private AudioClip introMusicaGigante;
    [SerializeField] private AudioClip loopMusicaGigante;

    [Space(5)]
    [Tooltip("Música do Gigante Ameaça Tripla - Intro (opcional) e Loop")]
    [SerializeField] private AudioClip introMusicaTripleThreatGigante;
    [SerializeField] private AudioClip loopMusicaTripleThreatGigante;

    [Header("Áudio do Anunciador (Vozes Arcade)")]
    [SerializeField] private AudioSource announcerAudioSource;
    [Range(0f, 1f)] public float volumeAnunciador = 0.8f;

    [Space(5)]
    public AudioClip vocStart;
    public AudioClip vocGiant;
    public AudioClip vocAccelerated;
    public AudioClip vocMutant;
    public AudioClip vocGiantMutant;
    public AudioClip vocGiantAccelerated;
    public AudioClip vocAcceleratedMutant;
    public AudioClip vocTripleThreat;

    private float currentSpawnRate;
    private float spawnTimer = 0f;
    private float difficultyTimer = 0f;
    private Camera mainCam;
    private ExplosionSpawner explosionSpawner;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        mainCam = Camera.main;
        explosionSpawner = GetComponent<ExplosionSpawner>();

        if (announcerAudioSource == null)
        {
            announcerAudioSource = GetComponent<AudioSource>();
            if (announcerAudioSource == null)
            {
                announcerAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        announcerAudioSource.playOnAwake = false;

        if (vocStart != null)
        {
            announcerAudioSource.PlayOneShot(vocStart, volumeAnunciador);
        }

        // Configura o timer para que o primeiro inimigo surja exatamente após atrasoPrimeiroSpawn segundos
        spawnTimer = GetCalculatedSpawnRate(1) - atrasoPrimeiroSpawn;

        // Inicia a trilha sonora principal da fase (Intro + Loop)
        if (MusicManager.Instance != null && (introMusicaFaseNormal != null || loopMusicaFaseNormal != null))
        {
            MusicManager.Instance.TrocarMusica(introMusicaFaseNormal, loopMusicaFaseNormal, volume: 0.1f);
        }
    }

    void Update()
    {
        if (enemies.Length == 0) return;

        difficultyTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        int currentLevel = GetCurrentLevel();
        currentSpawnRate = GetCalculatedSpawnRate(currentLevel);

        if (spawnTimer >= currentSpawnRate)
        {
            SpawnEnemyWave(currentLevel);
        }
    }

    public int GetCurrentLevel()
    {
        if (difficultyTimer > tempoNivelDificil) return 3;
        if (difficultyTimer > tempoNivelMedio) return 2;
        return 1;
    }

    private float GetCalculatedSpawnRate(int level)
    {
        float baseRate = spawnRateNivel1;
        if (level == 2) baseRate = spawnRateNivel2;
        else if (level == 3) baseRate = spawnRateNivel3;

        float fatorPericia = Mathf.Lerp(1.0f, 0.85f, PericiaNormalizada);
        return baseRate * fatorPericia;
    }

    private void SpawnEnemyWave(int currentLevel)
    {
        if (EnemyPool.Instance == null) return;

        spawnCount++;
        bool isEliteSpawn = (spawnCount % 5 == 0 && eliteEnemies.Length > 0);
        bool giganteSpawnou = false;
        float tempoDuracaoRajada = 0f;

        if (isEliteSpawn)
        {
            int randomEliteIndex = Random.Range(0, eliteEnemies.Length);
            GameObject elitePrefab = eliteEnemies[randomEliteIndex].prefab;

            EnemyBasics eb = SpawnSingleEnemyInstance(elitePrefab);
            if (eb != null && eb.IsGiant) giganteSpawnou = true;

            if (currentLevel == 3 && eb != null && !eb.IsGiant && !eb.IsMutant && !eb.IsAcelerated)
            {
                float chanceEliteGroup = chanceEliteGroupBase + (PericiaNormalizada * 0.15f);
                if (Random.value < chanceEliteGroup)
                {
                    int quantidadeExtra = Random.Range(1, maxInimigosElitesGrupo);
                    tempoDuracaoRajada = quantidadeExtra * intervaloSpawnGrupoElite;
                    float posXInicial = eb.transform.position.x;
                    StartCoroutine(SpawnStaggeredGroup(elitePrefab, quantidadeExtra, intervaloSpawnGrupoElite, forcePureNormal: true, posXInicial));
                }
            }
        }
        else
        {
            int randomIndex = Random.Range(0, enemies.Length);
            GameObject normalPrefab = enemies[randomIndex].prefab;

            EnemyBasics eb = SpawnSingleEnemyInstance(normalPrefab);
            if (eb != null && eb.IsGiant) giganteSpawnou = true;

            if (eb != null && !eb.IsGiant && !eb.IsMutant && !eb.IsAcelerated)
            {
                float chanceNormalGroup = chanceNormalGroupBase + (currentLevel * chanceNormalGroupPorNivel) + (PericiaNormalizada * 0.15f);
                if (Random.value < chanceNormalGroup)
                {
                    int quantidadeExtra = Random.Range(1, maxInimigosNormaisGrupo);
                    tempoDuracaoRajada = quantidadeExtra * intervaloSpawnGrupoNormal;
                    float posXInicial = eb.transform.position.x;
                    StartCoroutine(SpawnStaggeredGroup(normalPrefab, quantidadeExtra, intervaloSpawnGrupoNormal, forcePureNormal: true, posXInicial));
                }
            }
        }

        float delayExtra = giganteSpawnou ? giantSpawnDelay : 0f;
        spawnTimer = -tempoDuracaoRajada - delayExtra;
    }

    [Header("Suporte Durante o Gigante (Boss Escort)")]
    [Tooltip("Intervalo em segundos para spawnar inimigos rápidos de apoio durante a luta contra o Gigante")]
    [SerializeField] private float intervaloMinionsGigante = 3.5f;
    private Coroutine rotinaSuporteGigante;

    private void IniciarSuporteGigante(EnemyBasics gigante)
    {
        if (rotinaSuporteGigante != null) StopCoroutine(rotinaSuporteGigante);
        rotinaSuporteGigante = StartCoroutine(RotinaMinionsEscolta(gigante));
    }

    private IEnumerator RotinaMinionsEscolta(EnemyBasics gigante)
    {
        while (gigante != null && gigante.gameObject.activeInHierarchy)
        {
            yield return new WaitForSeconds(intervaloMinionsGigante);

            if (gigante == null || !gigante.gameObject.activeInHierarchy) break;

            if (enemies.Length > 0)
            {
                int index = Random.Range(0, enemies.Length);
                SpawnSingleEnemyInstance(enemies[index].prefab, forcePureNormal: true);
            }
        }
    }

    public void NotificarMorteGigante()
    {
        if (rotinaSuporteGigante != null)
        {
            StopCoroutine(rotinaSuporteGigante);
            rotinaSuporteGigante = null;
        }

        if (spawnTimer < 0f)
        {
            spawnTimer = 0f;
        }

        if (MusicManager.Instance != null && (introMusicaFaseNormal != null || loopMusicaFaseNormal != null))
        {
            MusicManager.Instance.TrocarMusica(introMusicaFaseNormal, loopMusicaFaseNormal, volume: 0.1f);
        }
    }

    private EnemyBasics SpawnSingleEnemyInstance(GameObject prefab, bool forcePureNormal = false, float? posicaoXReferencia = null)
    {
        GameObject enemyObj = EnemyPool.Instance.GetEnemy(prefab);
        if (enemyObj == null) return null;

        EnemyBasics eb = enemyObj.GetComponent<EnemyBasics>();

        if (eb != null)
        {
            if (forcePureNormal)
            {
                eb.Normal();
            }
            else
            {
                eb.ResetEnemy();
                VerificarETocarVozAnunciador(eb);
            }
        }

        float minX = mainCam.ViewportToWorldPoint(new Vector3(margemBordaViewport, 0, 0)).x;
        float maxX = mainCam.ViewportToWorldPoint(new Vector3(1f - margemBordaViewport, 0, 0)).x;

        if (eb != null && eb.IsGiant)
        {
            float centerX = mainCam.ViewportToWorldPoint(new Vector3(0.5f, 0, 0)).x;
            enemyObj.transform.position = new Vector2(centerX, posicaoYGigante);

            if (GiantHealthBarUI.Instance != null)
            {
                GiantHealthBarUI.Instance.AbrirBarraVida(eb, eb.gameObject.name.Replace("(Clone)", ""));
            }

            IniciarSuporteGigante(eb);
            TrocarMusicaBoss(eb);
        }
        else if (posicaoXReferencia.HasValue)
        {
            float targetX = Random.Range(posicaoXReferencia.Value - dispersaoGrupoX, posicaoXReferencia.Value + dispersaoGrupoX);
            float clampedX = Mathf.Clamp(targetX, minX, maxX);
            enemyObj.transform.position = new Vector2(clampedX, 6f);
        }
        else
        {
            enemyObj.transform.position = new Vector2(Random.Range(minX, maxX), 6f);
        }

        ApplyShootingPatterns(enemyObj);

        return eb;
    }

    private void TrocarMusicaBoss(EnemyBasics eb)
    {
        if (MusicManager.Instance == null || eb == null) return;

        if (eb.IsGiant && eb.IsAcelerated && eb.IsMutant)
        {
            if (loopMusicaTripleThreatGigante != null || introMusicaTripleThreatGigante != null)
            {
                MusicManager.Instance.TrocarMusica(introMusicaTripleThreatGigante, loopMusicaTripleThreatGigante, volume: 0.2f);
            }
            else if (loopMusicaGigante != null || introMusicaGigante != null)
            {
                MusicManager.Instance.TrocarMusica(introMusicaGigante, loopMusicaGigante, volume: 0.2f);
            }
        }
        else if (eb.IsGiant)
        {
            if (loopMusicaGigante != null || introMusicaGigante != null)
            {
                MusicManager.Instance.TrocarMusica(introMusicaGigante, loopMusicaGigante, volume: 0.2f);
            }
        }
    }

    private void VerificarETocarVozAnunciador(EnemyBasics eb)
    {
        if (eb == null || announcerAudioSource == null) return;

        bool g = eb.IsGiant;
        bool a = eb.IsAcelerated;
        bool m = eb.IsMutant;

        AudioClip clipParaTocar = null;

        if (g && a && m) clipParaTocar = vocTripleThreat;
        else if (g && a) clipParaTocar = vocGiantAccelerated;
        else if (g && m) clipParaTocar = vocGiantMutant;
        else if (a && m) clipParaTocar = vocAcceleratedMutant;
        else if (g) clipParaTocar = vocGiant;
        else if (a) clipParaTocar = vocAccelerated;
        else if (m) clipParaTocar = vocMutant;

        if (clipParaTocar != null)
        {
            announcerAudioSource.PlayOneShot(clipParaTocar, volumeAnunciador);
        }
    }

    private IEnumerator SpawnStaggeredGroup(GameObject prefab, int countExtra, float interval, bool forcePureNormal, float posicaoXInicial)
    {
        float posicaoAncoraX = posicaoXInicial;

        for (int i = 0; i < countExtra; i++)
        {
            yield return new WaitForSeconds(interval);
            EnemyBasics eb = SpawnSingleEnemyInstance(prefab, forcePureNormal, posicaoAncoraX);

            if (eb != null)
            {
                float minX = mainCam.ViewportToWorldPoint(new Vector3(margemBordaViewport, 0, 0)).x;
                float maxX = mainCam.ViewportToWorldPoint(new Vector3(1f - margemBordaViewport, 0, 0)).x;
                posicaoAncoraX = Mathf.Clamp(eb.transform.position.x, minX, maxX);
            }
        }
    }

    private void ApplyShootingPatterns(GameObject enemyObj)
    {
        int level = GetCurrentLevel();
        EnemyDifficultySettings[] allSettings = enemyObj.GetComponentsInChildren<EnemyDifficultySettings>();
        EnemyBasics eb = enemyObj.GetComponent<EnemyBasics>();

        foreach (var settings in allSettings)
        {
            var shooter = settings.GetComponent<BulletSpawner>();
            if (shooter == null) shooter = settings.GetComponentInChildren<BulletSpawner>();

            if (shooter != null && eb != null)
            {
                if (eb.IsMutant)
                {
                    shooter.ApplyPattern(settings.GetPattern(4));
                }
                else
                {
                    shooter.ApplyPattern(settings.GetPattern(level));
                }
            }

            if (eb != null && explosionSpawner != null)
            {
                eb.OnDeathExplosion = explosionSpawner.Explodir;
            }
        }
    }

    public void AdicionarPericia(float quantidade)
    {
        periciaJogador = Mathf.Clamp(periciaJogador + quantidade, minPericia, maxPericia);
    }

    public void RemoverPericia(float quantidade)
    {
        periciaJogador = Mathf.Clamp(periciaJogador - quantidade, minPericia, maxPericia);
    }

    public void RegistrarInimigoDerrotado() => AdicionarPericia(ganhoPorInimigo);
    public void RegistrarGraze() => AdicionarPericia(ganhoPorGraze);
    public void RegistrarMorteJogador() => RemoverPericia(perdaPorMorte);
    public void RegistrarColetavelPerdido() => RemoverPericia(perdaPorColetavelPerdido);
}