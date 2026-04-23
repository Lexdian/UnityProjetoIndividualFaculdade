using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance;

    private Dictionary<string, List<GameObject>> pools = new Dictionary<string, List<GameObject>>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public GameObject GetEnemy(GameObject prefab)
    {
        string key = prefab.name;

        if (!pools.ContainsKey(key))
        {
            pools.Add(key, new List<GameObject>());
        }

        foreach (GameObject enemy in pools[key])
        {
            if (!enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
                return enemy;
            }
        }

        GameObject newEnemy = Instantiate(prefab);
        pools[key].Add(newEnemy);
        return newEnemy;
    }
}