using System.Collections.Generic;
using UnityEngine;

public class ImpactPool : MonoBehaviour
{
    public static ImpactPool Instance { get; private set; }

    [Header("Configurações do Pool")]
    [SerializeField] private GameObject impactPrefab; // Prefab do efeito de impacto
    [SerializeField] private int initialPoolSize = 20; // Quantidade pré-instanciada ao iniciar

    private List<GameObject> poolList = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InicializarPool();
    }

    private void InicializarPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CriarNovoImpacto();
        }
    }

    private GameObject CriarNovoImpacto()
    {
        GameObject obj = Instantiate(impactPrefab, transform);
        obj.SetActive(false); // Mantém desativado até ser chamado
        poolList.Add(obj);
        return obj;
    }

    public GameObject SpawnImpact(Vector3 posicao, Quaternion rotacao)
    {
        // 1. Procura um efeito inativo na lista
        for (int i = 0; i < poolList.Count; i++)
        {
            if (!poolList[i].activeInHierarchy)
            {
                GameObject impact = poolList[i];
                impact.transform.position = posicao;
                impact.transform.rotation = rotacao;
                impact.SetActive(true);
                return impact;
            }
        }

        // 2. Se todos estiverem ativos na tela, expande a pool dinamicamente
        GameObject novoImpacto = CriarNovoImpacto();
        novoImpacto.transform.position = posicao;
        novoImpacto.transform.rotation = rotacao;
        novoImpacto.SetActive(true);
        return novoImpacto;
    }
    public GameObject SpawnImpact(Vector3 posicao)
    {
        // 1. Procura um efeito inativo na lista
        for (int i = 0; i < poolList.Count; i++)
        {
            if (!poolList[i].activeInHierarchy)
            {
                GameObject impact = poolList[i];
                impact.transform.position = posicao;
                impact.transform.rotation = Quaternion.Euler(0, 0, -90); // Define a rotação padrão
                impact.SetActive(true);
                return impact;
            }
        }

        // 2. Se todos estiverem ativos na tela, expande a pool dinamicamente
        GameObject novoImpacto = CriarNovoImpacto();
        novoImpacto.transform.position = posicao;
        novoImpacto.transform.rotation = Quaternion.Euler(0,0,-90); // Define a rotação padrão
        novoImpacto.SetActive(true);
        return novoImpacto;
    }
}