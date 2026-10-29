using UnityEngine;
using DG.Tweening;

public class ShrinkAndDestroy : MonoBehaviour
{
    [Header("Configurações de Encolhimento")]
    [Tooltip("Tempo em segundos que o objeto espera antes de começar a encolher")]
    [SerializeField] private float delayBeforeShrink = 2f;

    [Tooltip("Duração da animação de encolhimento")]
    [SerializeField] private float shrinkDuration = 0.5f;

    [Tooltip("Facilidade/Animação da curva do encolhimento")]
    [SerializeField] private Ease shrinkEase = Ease.InBack;

    [Tooltip("Se verdadeiro, usa Destroy(). Se falso, usa SetActive(false) para Object Pooling.")]
    [SerializeField] private bool destroyObject = true;

    private Vector3 originalScale;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // Reseta a escala inicial para suportar Object Pooling
        transform.localScale = originalScale;

        // Inicia o encolhimento via DOTween
        transform.DOScale(Vector3.zero, shrinkDuration)
            .SetDelay(delayBeforeShrink)
            .SetEase(shrinkEase)
            .OnComplete(FinalizarObjeto);
    }

    private void OnDisable()
    {
        // Interrompe a animação do DOTween para evitar leaks de memória ao desativar
        transform.DOKill();
    }

    private void FinalizarObjeto()
    {
        if (destroyObject)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}