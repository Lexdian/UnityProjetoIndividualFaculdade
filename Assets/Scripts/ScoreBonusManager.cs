using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScoreBonusManager : MonoBehaviour
{
    public static ScoreBonusManager Instance { get; private set; }

    [Header("Referências de UI")]
    [SerializeField] private Text textoScore;          // Texto do Score principal
    [SerializeField] private Text textoPendingScore;   // Texto secundário ("+100")

    [Header("Configurações de Animação")]
    [SerializeField] private float tempoEsperaCommit = 1.0f; // Tempo sem pegar itens para somar ao Score
    [SerializeField] private float duracaoSubida = 0.35f;    // Velocidade da subida até o Score
    [SerializeField] private float duracaoContagem = 0.5f;   // Duração da rolagem dos números do Score

    private int pendingScore = 0;       // Bônus acumulado no "+XX"
    private int pontosEmTransicao = 0; // Guarda os pontos enquanto o texto está subindo
    private int displayedScore = 0;    // Pontuação exibida no Score principal
    private int targetScore = 0;       // Pontuação final alvo da rolagem
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
        if (PlayerMovement.Instance != null)
        {
            displayedScore = PlayerMovement.Instance.score;
            targetScore = displayedScore;
        }

        if (textoPendingScore != null)
        {
            posicaoOriginalPending = textoPendingScore.transform.localPosition;
            textoPendingScore.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Força o commit instantâneo de todos os pontos pendentes/animando (usado na Morte do Jogador)
    /// </summary>
    public void CommitImmediate()
    {
        if (commitCoroutine != null) StopCoroutine(commitCoroutine);
        if (sequenciaSubida != null && sequenciaSubida.IsActive()) sequenciaSubida.Kill();
        if (countTween != null && countTween.IsActive()) countTween.Kill();

        int pontosRestantes = pendingScore + pontosEmTransicao;

        if (targetScore > displayedScore)
        {
            pontosRestantes += (targetScore - displayedScore);
        }

        pendingScore = 0;
        pontosEmTransicao = 0;

        int scoreFinal = (PlayerMovement.Instance != null ? PlayerMovement.Instance.score : displayedScore) + pontosRestantes;

        displayedScore = scoreFinal;
        targetScore = scoreFinal;

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.score = scoreFinal;
        }

        if (textoScore != null)
        {
            textoScore.text = scoreFinal.ToString("N0");
        }

        if (textoPendingScore != null)
        {
            textoPendingScore.gameObject.SetActive(false);
        }
    }

    public void AdicionarPontosPendentes(int pontos)
    {
        if (sequenciaSubida != null && sequenciaSubida.IsActive())
        {
            sequenciaSubida.Kill();
            pendingScore += pontosEmTransicao;
            pontosEmTransicao = 0;
        }

        pendingScore += pontos;

        if (textoPendingScore != null)
        {
            textoPendingScore.transform.DOKill();
            textoPendingScore.DOKill();

            textoPendingScore.transform.localPosition = posicaoOriginalPending;
            textoPendingScore.transform.localScale = Vector3.one;

            Color corAjustada = textoPendingScore.color;
            corAjustada.a = 1f;
            textoPendingScore.color = corAjustada;

            textoPendingScore.gameObject.SetActive(true);
            textoPendingScore.text = "+" + pendingScore.ToString("N0");

            textoPendingScore.transform.DOPunchScale(new Vector3(0.35f, 0.35f, 0f), 0.15f, 5, 0.5f);
        }

        if (commitCoroutine != null) StopCoroutine(commitCoroutine);
        commitCoroutine = StartCoroutine(AguardarEFundirPontos());
    }

    private IEnumerator AguardarEFundirPontos()
    {
        yield return new WaitForSeconds(tempoEsperaCommit);

        if (pendingScore <= 0) yield break;

        pontosEmTransicao = pendingScore;
        pendingScore = 0;

        sequenciaSubida = DOTween.Sequence();

        sequenciaSubida.Append(textoPendingScore.transform.DOLocalMoveY(textoScore.transform.localPosition.y, duracaoSubida).SetEase(Ease.InQuad));
        sequenciaSubida.Join(textoPendingScore.DOFade(0f, duracaoSubida));

        sequenciaSubida.OnComplete(() =>
        {
            FinalizarSubidaEFundir();
        });
    }

    private void FinalizarSubidaEFundir()
    {
        int pontosParaSomar = pontosEmTransicao;
        pontosEmTransicao = 0;

        textoPendingScore.gameObject.SetActive(false);
        textoPendingScore.transform.localPosition = posicaoOriginalPending;

        Color c = textoPendingScore.color;
        c.a = 1f;
        textoPendingScore.color = c;

        int scoreBase = (PlayerMovement.Instance != null) ? PlayerMovement.Instance.score : displayedScore;

        if (countTween != null && countTween.IsActive())
        {
            scoreBase = targetScore;
        }

        AnimarContagemScore(scoreBase, scoreBase + pontosParaSomar);
    }

    private void AnimarContagemScore(int valorInicial, int valorFinal)
    {
        if (countTween != null && countTween.IsActive())
        {
            countTween.Kill();
            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.score = targetScore;
            }
            displayedScore = targetScore;
        }

        targetScore = valorFinal;

        countTween = DOVirtual.Float(valorInicial, valorFinal, duracaoContagem, valorInterpolado =>
        {
            displayedScore = Mathf.RoundToInt(valorInterpolado);

            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.score = displayedScore;
            }

            if (textoScore != null)
            {
                textoScore.text = displayedScore.ToString("N0");
            }
        }).OnComplete(() =>
        {
            displayedScore = valorFinal;

            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.score = valorFinal;
            }

            if (textoScore != null)
            {
                textoScore.text = valorFinal.ToString("N0");
            }
        });
    }

    public void SetScoreInicial(int scoreInicial)
    {
        displayedScore = scoreInicial;
        targetScore = scoreInicial;

        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.score = scoreInicial;
        }

        if (textoScore != null) textoScore.text = displayedScore.ToString("N0");
    }
}