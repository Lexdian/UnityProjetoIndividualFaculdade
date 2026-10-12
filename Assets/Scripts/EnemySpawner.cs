using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public string name;
        public GameObject prefab;
    }

    [Header("Configurações de Spawn")]
    public EnemyType[] enemies;
    public EnemyType[] eliteEnemies;
    private int spawnCount = 0; // Contador de inimigos spawnados
    public float initialSpawnRate = 2f;    // Velocidade inicial
    public float minSpawnRate = 1f;        // Velocidade máxima permitida
    public float spawnDecay = 0.05f;       // Quanto diminui a cada novo inimigo

    [Header("Mecânica de Spawn Duplo")]
    [Range(0f, 1f)]
    [Tooltip("Chance (de 0 a 1) de spawnar 2 inimigos ao mesmo tempo")]
    public float doubleSpawnChance = 0.15f; // 15% de chance padrão

    [Header("Controle de Dificuldade (Tempo)")]
    [Tooltip("Tempo em segundos para os inimigos passarem para o Nível 2 (Médio)")]
    public float tempoNivelMedio = 20f;

    [Tooltip("Tempo em segundos para os inimigos passarem para o Nível 3 (Difícil)")]
    public float tempoNivelDificil = 40f;

    private float currentSpawnRate;
    private float spawnTimer = 0f;
    private float difficultyTimer = 0f;
    private Camera mainCam;

    private ExplosionSpawner explosionSpawner;

    void Start()
    {
        mainCam = Camera.main;
        currentSpawnRate = initialSpawnRate;

        // Dispara o primeiro spawn no primeiro frame
        spawnTimer = currentSpawnRate;

        explosionSpawner = gameObject.GetComponent<ExplosionSpawner>();
    }

    void Update()
    {
        if (enemies.Length == 0) return;

        difficultyTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnRate)
        {
            SpawnEnemy();

            // Reduz o tempo de espera para o próximo ciclo de spawn
            AccelerateSpawn();

            spawnTimer = 0f;
        }
    }

    void AccelerateSpawn()
    {
        if (currentSpawnRate > minSpawnRate)
        {
            currentSpawnRate -= spawnDecay;

            if (currentSpawnRate < minSpawnRate)
            {
                currentSpawnRate = minSpawnRate;
            }
        }
    }

    void SpawnEnemy()
    {
        if (EnemyPool.Instance == null) return;

        // 1. Determina quantos inimigos vão spawnar nesta leva
        int quantidadeParaSpawnar = (Random.value < doubleSpawnChance) ? 2 : 1;

        // 2. Executa o spawn a quantidade de vezes sorteada
        for (int i = 0; i < quantidadeParaSpawnar; i++)
        {
            InstantiateSingleEnemy();
        }
    }

    private void InstantiateSingleEnemy()
    {
        GameObject selectedPrefab = null;

        spawnCount++;

        // A cada 5 inimigos spawnados (ou a sua regra original), gera um elite se houver
        if (spawnCount % 5 == 0 && eliteEnemies.Length > 0)
        {
            int randomEliteIndex = Random.Range(0, eliteEnemies.Length);
            selectedPrefab = eliteEnemies[randomEliteIndex].prefab;
        }
        else
        {
            int randomIndex = Random.Range(0, enemies.Length);
            selectedPrefab = enemies[randomIndex].prefab;
        }

        GameObject enemyObj = EnemyPool.Instance.GetEnemy(selectedPrefab);

        enemyObj.GetComponent<EnemyBasics>().ResetEnemy(); // Resetando o inimigo antes de spawnar

        // Posicionamento na tela
        float minX = mainCam.ViewportToWorldPoint(new Vector3(0.1f, 0, 0)).x;
        float maxX = mainCam.ViewportToWorldPoint(new Vector3(0.9f, 0, 0)).x;
        enemyObj.transform.position = new Vector2(Random.Range(minX, maxX), 6f);

        // Define o nível baseado nos tempos configuráveis do Inspector
        int level = 1;
        if (difficultyTimer > tempoNivelDificil)
        {
            level = 3;
        }
        else if (difficultyTimer > tempoNivelMedio)
        {
            level = 2;
        }

        // Aplicação dos padrões de dificuldade
        EnemyDifficultySettings[] allSettings = enemyObj.GetComponentsInChildren<EnemyDifficultySettings>();

        foreach (var settings in allSettings)
        {
            var shooter = settings.GetComponent<BulletSpawner>();

            if (shooter == null)
            {
                shooter = settings.GetComponentInChildren<BulletSpawner>();
            }

            if (shooter != null)
            {
                shooter.ApplyPattern(settings.GetPattern(level));
            }
        }

        enemyObj.GetComponent<EnemyBasics>().OnDeathExplosion = explosionSpawner.Explodir;
    }
}