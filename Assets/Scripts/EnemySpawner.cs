using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Pool")]
    public EnemyPool pool;

    [Header("Spawn")]
    public float spawnRate = 2f;
    public float minSpawnRate = 0.5f;
    public float difficultyIncreaseRate = 0.05f;

    public float spawnRangeX = 8f;
    public float spawnY = 6f;

    [Header("Movimento")]
    public AbstractEnemyMovement[] movementOptions;

    private float timer = 0f;
    private float difficultyTimer = 0f;

    void Start()
    {
        pool.Initialize();

        SpawnEnemy();
    }

    void Update()
    {
        timer += Time.deltaTime;
        difficultyTimer += Time.deltaTime;

        if (timer >= spawnRate)
        {
            SpawnEnemy();
            timer = 0f;
        }

        if (spawnRate > minSpawnRate)
        {
            spawnRate -= difficultyIncreaseRate * Time.deltaTime;
        }
    }

    void SpawnEnemy()
    {
        GameObject enemyObj = pool.GetEnemy();

        float x = Random.Range(-spawnRangeX, spawnRangeX);
        enemyObj.transform.position = new Vector2(x, spawnY);

        EnemyMovement movement = enemyObj.GetComponent<EnemyMovement>();

        int randomIndex = Random.Range(0, movementOptions.Length);
        movement.Setup(movementOptions[randomIndex]);

        enemyObj.GetComponent<EnemyBasics>().Health = 3;

        BulletSpawner shooter = enemyObj.GetComponentInChildren<BulletSpawner>();

        if (shooter != null)
        {
            float difficulty = difficultyTimer;
            int level = Mathf.FloorToInt(difficulty / 10f);

            int directions = 4;
            int burst = 1;

            if (level >= 1)
            {
                directions = Random.Range(4, 6);
                burst = Random.Range(1, 2);
            }

            if (level >= 2)
            {
                directions = Random.Range(5, 7);
                burst = Random.Range(1, 3);
            }

            if (level >= 3)
            {
                directions = Random.Range(6, 9);
                burst = Random.Range(2, 4);
            }

            if (level >= 4)
            {
                directions = Random.Range(8, 12);
                burst = Random.Range(2, 5);
            }

            float fireRate = Mathf.Max(0.3f, 1.5f - level * 0.2f);
            float rotation = Random.Range(-1f, 1f) * (20f + level * 10f);
            float bulletSpeed = 1f + level * 0.5f;

            shooter.Setup(directions, burst, fireRate, rotation, bulletSpeed);

            shooter.ResetFireTimer(Random.Range(0f, fireRate));
        }
    }
}