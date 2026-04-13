using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public GameObject enemyPrefab;
    public int poolSize = 20;

    private List<GameObject> pool = new List<GameObject>();

    public void Initialize()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            pool.Add(enemy);
        }
    }

    public GameObject GetEnemy()
    {
        foreach (GameObject enemy in pool)
        {
            if (!enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
                return enemy;
            }
        }

        // Se acabar, cria mais (opcional)
        GameObject newEnemy = Instantiate(enemyPrefab);
        pool.Add(newEnemy);
        return newEnemy;
    }
}