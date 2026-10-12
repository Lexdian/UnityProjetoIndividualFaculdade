using System.Collections.Generic;
using UnityEngine;

public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [Header("Configurações do Prefab e Pool")]
    [SerializeField] private GameObject floatingTextPrefab;
    [SerializeField] private int initialPoolSize = 15;

    private List<FloatingText> pool = new List<FloatingText>();

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
            CriarNovoTexto();
        }
    }

    private FloatingText CriarNovoTexto()
    {
        GameObject obj = Instantiate(floatingTextPrefab, transform);
        obj.SetActive(false);
        FloatingText ft = obj.GetComponent<FloatingText>();
        pool.Add(ft);
        return ft;
    }

    private FloatingText ObterTextoDaPool()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeInHierarchy)
            {
                return pool[i];
            }
        }
        return CriarNovoTexto(); // Expande se necessário
    }

    // --- MÉTODOS PÚBLICOS ESTÁTICOS DE FÁCIL ACESSO ---

    /// <summary>
    /// Exibe um texto flutuante na posição informada.
    /// </summary>
    public static void Show(string message, Vector3 worldPosition, Color color, float fontSize = 0f)
    {
        if (Instance == null) return;

        FloatingText ft = Instance.ObterTextoDaPool();
        ft.transform.position = worldPosition;
        ft.gameObject.SetActive(true);
        ft.Setup(message, color, fontSize);
    }

    /// <summary>
    /// Sobrecarga simplificada (Usa cor branca padrão).
    /// </summary>
    public static void Show(string message, Vector3 worldPosition)
    {
        Show(message, worldPosition, Color.white);
    }
}