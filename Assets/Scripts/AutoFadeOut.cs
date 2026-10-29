using System.Collections;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(SpriteRenderer))]
public class AutoFadeOut : MonoBehaviour
{
    [Header("Configurações do Fade")]
    [SerializeField] private float tempoVidaVisivel = 3.0f; // Tempo que o objeto fica visível antes de sumir
    [SerializeField] private float duracaoFade = 0.8f;     // Duração do desvanecimento
    [SerializeField] private bool destruirAoFinal = false;  // Marque TRUE se não usar Object Pooling

    private SpriteRenderer spriteRenderer;
    private Color corOriginal;
    private Tween fadeTween;
    private Coroutine timerCoroutine;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            corOriginal = spriteRenderer.color;
        }
    }

    private void OnEnable()
    {
        // Reseta o estado inicial sempre que o objeto é ativado no pool
        if (spriteRenderer != null)
        {
            spriteRenderer.DOKill();
            Color corAjustada = corOriginal;
            corAjustada.a = 1f; // Reseta transparência para 100%
            spriteRenderer.color = corAjustada;
        }

        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        timerCoroutine = StartCoroutine(IniciarContagemFade());
    }

    public IEnumerator IniciarContagemFade()
    {
        yield return new WaitForSeconds(tempoVidaVisivel);

        if (spriteRenderer != null)
        {
            // Transiciona o Alpha para 0 usando DOTween
            fadeTween = spriteRenderer.DOFade(0f, duracaoFade)
                .SetEase(Ease.OutQuad)
                .OnComplete(FinalizarObjeto);
        }
        else
        {
            FinalizarObjeto();
        }
    }

    private void FinalizarObjeto()
    {
        if (destruirAoFinal)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false); // Retorna ao pool
        }
    }

    private void OnDisable()
    {
        // Cancela corrotinas e tweens ao desativar para evitar memory leaks
        if (timerCoroutine != null) StopCoroutine(timerCoroutine);
        fadeTween?.Kill();
        spriteRenderer?.DOKill();
    }
}