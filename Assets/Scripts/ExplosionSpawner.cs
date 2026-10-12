using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionSpawner : MonoBehaviour
{
    [Header("Configurações da Pool")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private int poolSize = 20;

    [Header("Configurações da Explosão")]

    [Tooltip("Tempo de espera entre cada explosão (0 para instantâneo)")]
    [SerializeField] private float delayBetweenExplosions = 0.05f;

    private List<GameObject> pool = new List<GameObject>();

    private void Awake()
    {
        InicializarPool();
    }

    private void InicializarPool()
    {
        if (explosionPrefab == null)
        {
            Debug.LogError("ExplosionPrefab não configurado no Inspector!", this);
            return;
        }

        // Instancia os objetos da pool assim que o Spawner é criado
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(explosionPrefab, transform);
            obj.SetActive(false);
            pool.Add(obj);
        }
    }

    /// <summary>
    /// Pega uma explosão inativa da Pool. Se não houver, expande a pool dinamicamente.
    /// </summary>
    private GameObject GetExplosionFromPool()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].activeInHierarchy)
            {
                return pool[i];
            }
        }

        // Expansão de segurança caso precise de mais explosões do que o tamanho inicial
        GameObject extraObj = Instantiate(explosionPrefab, transform);
        extraObj.SetActive(false);
        pool.Add(extraObj);
        return extraObj;
    }

    /// <summary>
    /// Dispara o efeito de explosão na posição especificada.
    /// </summary>
    public void Explodir(Vector2 position, int minExplosions = 3, int maxExplosions = 7, float offsetRadius = 1.5f)
    {
        int totalExplosions = Random.Range(minExplosions, maxExplosions + 1);

        if (delayBetweenExplosions > 0f)
        {
            StartCoroutine(SpawnExplosionsRoutine(position, totalExplosions, offsetRadius));
        }
        else
        {
            for (int i = 0; i < totalExplosions; i++)
            {
                SpawnSingleExplosion(position, offsetRadius);
            }
        }
    }

    private IEnumerator SpawnExplosionsRoutine(Vector2 basePosition, int count, float offsetRadius)
    {
        WaitForSeconds wait = new WaitForSeconds(delayBetweenExplosions);

        for (int i = 0; i < count; i++)
        {
            SpawnSingleExplosion(basePosition, offsetRadius);
            yield return wait;
        }
    }

    private void SpawnSingleExplosion(Vector2 basePosition, float offsetRadius)
    {
        GameObject explosion = GetExplosionFromPool();

        if (explosion != null)
        {
            Vector2 randomOffset = Random.insideUnitCircle * offsetRadius;
            Vector3 spawnPosition = new Vector3(basePosition.x + randomOffset.x, basePosition.y + randomOffset.y, 0f);

            explosion.transform.position = spawnPosition;
            explosion.SetActive(true);
        }
    }
}