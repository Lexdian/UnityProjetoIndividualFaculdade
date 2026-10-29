using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    private readonly CultureInfo culturaPontos = new CultureInfo("pt-BR");

    [Header("UI Elements")]
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text scoreValueText;
    [SerializeField] private TMP_Text highScoreLabel;
    [SerializeField] private TMP_Text highScoreValueText;
    [SerializeField] private TMP_Text newHighScoreText;

    [Header("Botões")]
    [SerializeField] private RectTransform containerBotoes;
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button leaderboardButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Áudio da Contagem")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip somContagemScore;
    [SerializeField] private float pitchInicial = 0.8f;
    [SerializeField] private float pitchFinal = 1.5f;

    [Header("Configuração de Duração Dinâmica da Contagem")]
    [SerializeField] private float duracaoMinimaScore = 0.8f;
    [SerializeField] private float duracaoMaximaScore = 3.5f;
    [Tooltip("Pontuação necessária para atingir a duração máxima de contagem")]
    [SerializeField] private float scoreParaDuracaoMaxima = 100000f;

    private float animationDuration = 0.8f;
    private string leaderboardId = "InfinityScore";

    private Vector3 gameOverTargetPos;
    private Vector3 scoreValueTargetPos;

    private CanvasGroup playAgainCanvasGroup;
    private CanvasGroup leaderboardCanvasGroup;
    private CanvasGroup mainMenuCanvasGroup;

    private void Awake()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (gameOverText != null)
            gameOverTargetPos = gameOverText.transform.localPosition;

        if (scoreValueText != null)
            scoreValueTargetPos = scoreValueText.transform.localPosition;

        playAgainCanvasGroup = GetOrAddCanvasGroup(playAgainButton);
        leaderboardCanvasGroup = GetOrAddCanvasGroup(leaderboardButton);
        mainMenuCanvasGroup = GetOrAddCanvasGroup(mainMenuButton);
    }

    private CanvasGroup GetOrAddCanvasGroup(Button btn)
    {
        if (btn == null) return null;
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
        return cg;
    }

    void Start()
    {
        SetupInitialState();
        StartGameOverSequence();
        CarregarDadosLeaderboardAsync();
    }

    private string FormatarPontos(int valor)
    {
        return valor.ToString("#,##0", culturaPontos);
    }

    private async void CarregarDadosLeaderboardAsync()
    {
        try
        {
            if (UGSManager.Instance != null && GameManager.Instance != null)
            {
                await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
                _ = UGSManager.Instance.AddScore(GameManager.Instance.HighScore);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Aviso UGS Leaderboard: {e.Message}");
        }
    }

    private void SetupInitialState()
    {
        // 1. Game Over inicia no centro e grande
        gameOverText.transform.localPosition = Vector3.zero;
        gameOverText.transform.localScale = Vector3.one * 1.5f;
        gameOverText.color = SetAlpha(gameOverText.color, 1f);

        // 2. Score Value inicia no centro e grande com formatação
        scoreValueText.transform.localPosition = Vector3.zero;
        scoreValueText.transform.localScale = Vector3.one * 1.8f;
        scoreValueText.color = SetAlpha(scoreValueText.color, 0f);
        scoreValueText.text = FormatarPontos(0);

        scoreLabel.color = SetAlpha(scoreLabel.color, 0f);
        highScoreLabel.color = SetAlpha(highScoreLabel.color, 0f);
        highScoreValueText.color = SetAlpha(highScoreValueText.color, 0f);

        int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;
        highScoreValueText.text = FormatarPontos(highScore);

        newHighScoreText.gameObject.SetActive(false);

        // 3. Força os botões para escala 1 temporariamente para o Layout Group posicionar cada um corretamente
        if (playAgainButton != null) playAgainButton.transform.localScale = Vector3.one;
        if (leaderboardButton != null) leaderboardButton.transform.localScale = Vector3.one;
        if (mainMenuButton != null) mainMenuButton.transform.localScale = Vector3.one;

        if (containerBotoes != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(containerBotoes);
        }
        Canvas.ForceUpdateCanvases();

        // 4. Reseta alpha e escala para 0 com as posições preservadas
        ResetButtonState(playAgainButton, playAgainCanvasGroup);
        ResetButtonState(leaderboardButton, leaderboardCanvasGroup);
        ResetButtonState(mainMenuButton, mainMenuCanvasGroup);
    }

    private void ResetButtonState(Button btn, CanvasGroup cg)
    {
        if (btn == null) return;
        if (cg != null) cg.alpha = 0f;
        btn.transform.localScale = Vector3.zero;
    }

    private float CalcularDuracaoContagemScore(int score)
    {
        if (score <= 0) return duracaoMinimaScore;

        float t = Mathf.Clamp01((float)score / scoreParaDuracaoMaxima);
        return Mathf.Lerp(duracaoMinimaScore, duracaoMaximaScore, Mathf.Sqrt(t));
    }

    private void StartGameOverSequence()
    {
        Sequence s = DOTween.Sequence();

        // PASSO 1: Move "GAME OVER" do centro para o topo
        s.Append(gameOverText.transform.DOLocalMove(gameOverTargetPos, animationDuration).SetEase(Ease.OutQuart));
        s.Join(gameOverText.transform.DOScale(1f, animationDuration));

        // PASSO 2: Contagem de pontos com duração dinâmica, som dinâmico e formatação de milhar
        int targetScore = GameManager.Instance != null ? GameManager.Instance.currentScore : 0;
        int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;
        int displayedScore = 0;

        float duracaoContagem = CalcularDuracaoContagemScore(targetScore);

        s.Append(scoreValueText.DOFade(1f, 0.3f));

        s.AppendCallback(() => {
            if (audioSource != null && somContagemScore != null && targetScore > 0)
            {
                audioSource.clip = somContagemScore;
                audioSource.loop = true;
                audioSource.pitch = pitchInicial;
                audioSource.Play();
            }
        });

        s.Append(DOTween.To(() => displayedScore, x => displayedScore = x, targetScore, duracaoContagem)
            .OnUpdate(() => {
                scoreValueText.text = FormatarPontos(displayedScore);

                if (audioSource != null && audioSource.isPlaying && targetScore > 0)
                {
                    float progresso = (float)displayedScore / targetScore;
                    audioSource.pitch = Mathf.Lerp(pitchInicial, pitchFinal, progresso);
                }
            })
            .OnComplete(() => {
                scoreValueText.text = FormatarPontos(targetScore);

                if (audioSource != null && audioSource.isPlaying)
                {
                    audioSource.Stop();
                    audioSource.pitch = 1f;
                }
            }));

        // PASSO 3: Move o Score para o canto e faz surgir a label "SCORE"
        s.Append(scoreValueText.transform.DOLocalMove(scoreValueTargetPos, 0.6f).SetEase(Ease.OutCubic));
        s.Join(scoreValueText.transform.DOScale(1f, 0.6f));
        s.Join(scoreLabel.DOFade(1f, 0.5f));

        // PASSO 4: HighScore
        s.Append(highScoreLabel.DOFade(1f, 0.4f));
        s.AppendInterval(0.25f);
        s.Append(highScoreValueText.DOFade(1f, 0.4f));

        // PASSO 5: Recorde Novo
        s.AppendCallback(() => {
            if (targetScore > highScore)
            {
                newHighScoreText.gameObject.SetActive(true);
                newHighScoreText.color = SetAlpha(newHighScoreText.color, 0f);
                newHighScoreText.DOFade(1f, 0.3f);
                newHighScoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f);
            }
        });

        s.AppendInterval(0.3f);

        // PASSO 6: Botão Play Again surge PRIMEIRO
        s.Append(playAgainButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        if (playAgainCanvasGroup != null) s.Join(playAgainCanvasGroup.DOFade(1f, 0.25f));
        s.Append(playAgainButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 5, 0.5f));

        // PASSO 7: Botões Leaderboard e Main Menu surgem JUNTOS
        s.Append(leaderboardButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        if (leaderboardCanvasGroup != null) s.Join(leaderboardCanvasGroup.DOFade(1f, 0.25f));

        s.Join(mainMenuButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        if (mainMenuCanvasGroup != null) s.Join(mainMenuCanvasGroup.DOFade(1f, 0.25f));

        s.Append(leaderboardButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 5, 0.5f));
        s.Join(mainMenuButton.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0f), 0.3f, 5, 0.5f));
    }

    private Color SetAlpha(Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }
}