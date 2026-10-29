using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextColorOscillatorDOTween : MonoBehaviour
{
    [Header("Referência")]
    [Tooltip("Deixe vazio para buscar automaticamente no mesmo GameObject.")]
    [SerializeField] private TMP_Text tmpText;

    [Header("Cores da Oscilação")]
    [SerializeField] private Color color1 = Color.white;
    [SerializeField] private Color color2 = Color.cyan;

    [Header("Configurações")]
    [SerializeField, Tooltip("Tempo (em segundos) para ir de uma cor à outra.")] private float duration = 1f;
    [SerializeField, Tooltip("Suavização do movimento nas pontas.")] private Ease easeType = Ease.InOutSine;

    private Tween colorTween;

    void Awake()
    {
        if (tmpText == null)
        {
            tmpText = GetComponent<TMP_Text>();
        }
    }

    void Start()
    {
        if (tmpText == null) return;

        // Define a cor inicial
        tmpText.color = color1;

        // Anima para a color2, faz loop infinito (-1) do tipo Yoyo (vai e volta)
        colorTween = tmpText.DOColor(color2, duration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(easeType);
    }

    void OnDestroy()
    {
        // Boa prática para evitar memory leaks ao destruir o objeto
        colorTween?.Kill();
    }
}