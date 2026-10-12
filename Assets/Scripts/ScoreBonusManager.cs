using System.Collections;
using UnityEngine;
using UnityEngine.UI; // Substitua por TMPro se estiver usando TextMeshPro (TMP_Text)
using DG.Tweening;

public class ScoreBonusManager : MonoBehaviour
{
    public static ScoreBonusManager Instance { get; private set; }

    [Header("Referências de UI")]
    [SerializeField] private Text textoScore;          // O texto do Score principal
    [SerializeField] private Text textoPendingScore;   // O texto secundário logo abaixo ("+100")

    [Header("Configurações de Animação")]
    [SerializeField] private float tempoEsperaCommit = 1.0f; // Tempo sem pegar itens para somar ao Score
    [SerializeField] private float duracaoSubida = 0.35f;    // Velocidade da subida até o Score
    [SerializeField] private float duracaoContagem = 0.5f;  // Duração da rolagem dos números do Score

    private int pendingScore = 0;      // Bônus acumulado atualmente no "+XX"
    private int displayedScore = 0;    // Pontuação exibida no Score principal
    private Vector3 posicaoOriginalPending;
    private Coroutine commitCoroutine;
    private Sequence sequenciaSubida;
    private Tween countTween;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (textoPendingScore != null)
        {
            // Salva a posição local inicial abaixo do Score principal
            posicaoOriginalPending = textoPendingScore.transform.localPosition;
            textoPendingScore.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Chama esse método sempre que o jogador coletar um item
    /// </summary>
    public void AdicionarPontosPendentes(int pontos)
    {
        pendingScore += pontos;

        if (textoPendingScore != null)
        {
            // 1. Cancela e limpa qualquer animação do DOTween no texto pendente
            if (sequenciaSubida != null && sequenciaSubida.IsActive())
            {
                sequenciaSubida.Kill();
            }
            textoPendingScore.transform.DOKill();
            textoPendingScore.DOKill();

            // 2. Reseta imediatamente posição, opacidade e escala para o estado padrão
            textoPendingScore.transform.localPosition = posicaoOriginalPending;
            textoPendingScore.transform.localScale = Vector3.one;

            Color corAjustada = textoPendingScore.color;
            corAjustada.a = 1f;
            textoPendingScore.color = corAjustada;

            // 3. Atualiza o texto e ativa o GameObject
            textoPendingScore.gameObject.SetActive(true);
            textoPendingScore.text = "+" + pendingScore.ToString("N0");

            // 4. Efeito de PULSAR centralizado
            textoPendingScore.transform.DOPunchScale(new Vector3(0.35f, 0.35f, 0f), 0.15f, 5, 0.5f);
        }

        // Reinicia o cronômetro para esperar o jogador parar de coletar
        if (commitCoroutine != null) StopCoroutine(commitCoroutine);
        commitCoroutine = StartCoroutine(AguardarEFundirPontos());
    }

    private IEnumerator AguardarEFundirPontos()
    {
        // Aguarda o tempo de inatividade configurado
        yield return new WaitForSeconds(tempoEsperaCommit);

        if (pendingScore <= 0) yield break;

        int pontosParaSomar = pendingScore;
        pendingScore = 0; // Zera o acumulador pendente

        // --- ANIMAÇÃO DE SUBIDA E FADE OUT ---
        sequenciaSubida = DOTween.Sequence();

        // Faz o texto subir até a altura Y do Score principal e ficar transparente
        sequenciaSubida.Append(textoPendingScore.transform.DOLocalMoveY(textoScore.transform.localPosition.y, duracaoSubida).SetEase(Ease.InQuad));
        sequenciaSubida.Join(textoPendingScore.DOFade(0f, duracaoSubida));

        sequenciaSubida.OnComplete(() =>
        {
            // Reseta o texto pendente para o estado original
            textoPendingScore.gameObject.SetActive(false);
            textoPendingScore.transform.localPosition = posicaoOriginalPending;

            Color c = textoPendingScore.color;
            c.a = 1f;
            textoPendingScore.color = c;

            // Rola a contagem dos dígitos no Score Principal
            AnimarContagemScore(displayedScore, displayedScore + pontosParaSomar);
        });
    }

    private void AnimarContagemScore(int valorInicial, int valorFinal)
    {
        countTween?.Kill();

        // DOVirtual.Float interpola o valor entre o número antigo e o novo
        countTween = DOVirtual.Float(valorInicial, valorFinal, duracaoContagem, valorInterpolado =>
        {
            displayedScore = Mathf.RoundToInt(valorInterpolado);
            textoScore.text = displayedScore.ToString("N0");
        }).OnComplete(() =>
        {
            displayedScore = valorFinal;
            textoScore.text = displayedScore.ToString("N0");
        });
    }

    public void SetScoreInicial(int scoreInicial)
    {
        displayedScore = scoreInicial;
        if (textoScore != null) textoScore.text = displayedScore.ToString("N0");
    }
}