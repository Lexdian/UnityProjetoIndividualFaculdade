using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public GameObject bulletPrefab;

    private Queue<GameObject> pool = new Queue<GameObject>();

    public void Initialize(int poolSize)
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            pool.Enqueue(bullet);
        }
    }

    public GameObject GetBullet()
    {
        if (pool.Count > 0)
        {
            GameObject bullet = pool.Dequeue();
            bullet.SetActive(true);
            return bullet;
        }

        // fallback (segurança)
        GameObject newBullet = Instantiate(bulletPrefab);
        return newBullet;
    }

    public void ReturnBullet(GameObject bullet)
    {
        bullet.SetActive(false);
        pool.Enqueue(bullet);
    }
}