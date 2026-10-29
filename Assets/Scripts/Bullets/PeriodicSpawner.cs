using UnityEngine;

public class PeriodicSpawner : MonoBehaviour
{
    [Header("Configurações do Spawner")]
    [Tooltip("Prefab que será instanciado")]
    [SerializeField] private GameObject prefabToSpawn;

    [Tooltip("Intervalo em segundos entre cada spawn")]
    [SerializeField] private float interval = 3f;

    [Tooltip("Se marcado, dispara um spawn no exato momento em que o objeto é ativado")]
    [SerializeField] private bool spawnImmediately = false;

    private float timer = 0f;
    private bool isGiant = false;

    public void SetGiant(bool giant)
    {
        isGiant = giant;
    }

    private void OnEnable()
    {
        timer = 0f;
        isGiant = false;

        // Tenta identificar se este spawner pertence a um Inimigo ou Projétil Gigante
        if (TryGetComponent<EnemyBasics>(out EnemyBasics eb))
        {
            isGiant = eb.IsGiant;
        }
        else if (TryGetComponent<BulletMovement>(out BulletMovement bm))
        {
            isGiant = bm.IsGiant;
        }

        if (spawnImmediately)
        {
            SpawnObject();
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= interval)
        {
            timer = 0f;
            SpawnObject();
        }
    }

    private void SpawnObject()
    {
        if (prefabToSpawn != null)
        {
            GameObject spawnedObj = Instantiate(prefabToSpawn, transform.position, transform.rotation);

            // Determina se o spawner atual é gigante por variável ou componente
            bool serGigante = isGiant;
            if (!serGigante && TryGetComponent<EnemyBasics>(out EnemyBasics eb)) serGigante = eb.IsGiant;
            if (!serGigante && TryGetComponent<BulletMovement>(out BulletMovement bm)) serGigante = bm.IsGiant;

            // Repassa o estado de Gigante para o novo objeto criado
            if (spawnedObj.TryGetComponent<BulletMovement>(out BulletMovement spawnedBm))
            {
                spawnedBm.SetGiant(serGigante);
            }

            if (spawnedObj.TryGetComponent<SidewaysShooter>(out SidewaysShooter shooter))
            {
                shooter.SetGiant(serGigante);
            }

            if (spawnedObj.TryGetComponent<PeriodicSpawner>(out PeriodicSpawner subSpawner))
            {
                subSpawner.SetGiant(serGigante);
            }
        }
    }
}