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
    public float initialSpawnRate = 2f;    // Velocidade inicial
    public float minSpawnRate = 0.5f;      // Velocidade máxima permitida
    public float spawnDecay = 0.05f;       // Quanto diminui a cada novo inimigo

    private float currentSpawnRate;
    private float spawnTimer = 0f;
    private float difficultyTimer = 0f;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        currentSpawnRate = initialSpawnRate;

        // Regra: Não esperar o valor inicial para o primeiro inimigo
        // Forçamos o timer a ser igual ao rate para disparar no primeiro frame do Update
        spawnTimer = currentSpawnRate;
    }

    void Update()
    {
        if (enemies.Length == 0) return;

        difficultyTimer += Time.deltaTime;
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= currentSpawnRate)
        {
            SpawnEnemy();

            // Reduz o tempo de espera para o próximo inimigo
            AccelerateSpawn();

            spawnTimer = 0f;
        }
    }

    void AccelerateSpawn()
    {
        // Diminui o tempo atual, mas garante que não ultrapasse o mínimo
        if (currentSpawnRate > minSpawnRate)
        {
            currentSpawnRate -= spawnDecay;

            // Trava no valor mínimo para não ficar negativo ou impossível
            if (currentSpawnRate < minSpawnRate)
            {
                currentSpawnRate = minSpawnRate;
            }
        }
    }

    void SpawnEnemy()
    {
        if (EnemyPool.Instance == null) return;

        int randomIndex = Random.Range(0, enemies.Length);
        GameObject selectedPrefab = enemies[randomIndex].prefab;

        GameObject enemyObj = EnemyPool.Instance.GetEnemy(selectedPrefab);

        // Posicionamento na tela
        float minX = mainCam.ViewportToWorldPoint(new Vector3(0.1f, 0, 0)).x;
        float maxX = mainCam.ViewportToWorldPoint(new Vector3(0.9f, 0, 0)).x;
        enemyObj.transform.position = new Vector2(Random.Range(minX, maxX), 6f);

        // Define o nível baseado no tempo (20s = médio, 40s = difícil)
        int level = 1;
        if (difficultyTimer > 20f) level = 2;
        if (difficultyTimer > 40f) level = 3;

        var settings = enemyObj.GetComponent<EnemyDifficultySettings>();
        var shooter = enemyObj.GetComponentInChildren<BulletSpawner>();

        if (settings != null && shooter != null)
        {
            shooter.ApplyPattern(settings.GetPattern(level));
        }
    }
}