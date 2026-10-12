using System.Threading.Tasks;
using TMPro;
using Unity.Services.Leaderboards;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text gameOverText;
    [SerializeField] private TMP_Text scoreLabel;
    [SerializeField] private TMP_Text scoreValueText;
    [SerializeField] private TMP_Text highScoreLabel;
    [SerializeField] private TMP_Text highScoreValueText;
    [SerializeField] private TMP_Text newHighScoreText;

    [Header("Botões")]
    [SerializeField] private Button playAgainButton;
    [SerializeField] private Button leaderboardButton; // Atualizado para o botão de Leaderboard

    private float animationDuration = 0.8f;
    private string leaderboardId = "InfinityScore";

    async void Start()
    {
        SetupInitialState();

        // Lógica de dados UGS
        var playerScoreEntry = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
        _ = UGSManager.Instance.AddScore(GameManager.Instance.HighScore);

        StartGameOverSequence();
    }

    private void SetupInitialState()
    {
        // 1. Reset Game Over
        gameOverText.transform.localPosition = Vector3.zero;
        gameOverText.transform.localScale = Vector3.one * 1.5f;
        gameOverText.color = SetAlpha(gameOverText.color, 1);

        // 2. Reset Textos (Alpha em 0)
        scoreLabel.color = SetAlpha(scoreLabel.color, 0);
        scoreValueText.color = SetAlpha(scoreValueText.color, 0);
        highScoreLabel.color = SetAlpha(highScoreLabel.color, 0);
        highScoreValueText.color = SetAlpha(highScoreValueText.color, 0);

        // Inicializa valores
        scoreValueText.text = "0";
        highScoreValueText.text = GameManager.Instance.HighScore.ToString();

        // 3. Reset New High Score e Botões (Escala 0 para ficarem invisíveis)
        newHighScoreText.gameObject.SetActive(false);
        playAgainButton.transform.localScale = Vector3.zero;
        leaderboardButton.transform.localScale = Vector3.zero;
    }

    private void StartGameOverSequence()
    {
        Sequence s = DOTween.Sequence();

        // A. Move Título para cima
        s.Append(gameOverText.transform.DOLocalMoveY(200f, animationDuration).SetEase(Ease.OutQuart));
        s.Join(gameOverText.transform.DOScale(1f, animationDuration));

        // B. Aparece Labels e Valores
        s.Append(scoreLabel.DOFade(1f, 0.5f));
        s.Join(scoreValueText.DOFade(1f, 0.5f));
        s.Join(highScoreLabel.DOFade(1f, 0.5f));
        s.Join(highScoreValueText.DOFade(1f, 0.5f));

        // C. Animação de preenchimento do Score
        int targetScore = GameManager.Instance.currentScore;
        int displayedScore = 0;

        s.Append(DOTween.To(() => displayedScore, x => displayedScore = x, targetScore, 1.5f)
            .OnUpdate(() => {
                scoreValueText.text = displayedScore.ToString();
            }));

        // D. Checagem e Animação do "New High Score"
        s.AppendCallback(() => {
            if (GameManager.Instance.currentScore > GameManager.Instance.HighScore)
            {
                newHighScoreText.gameObject.SetActive(true);
                newHighScoreText.color = SetAlpha(newHighScoreText.color, 0);
                newHighScoreText.DOFade(1f, 0.3f);
                newHighScoreText.transform.DOPunchScale(Vector3.one * 0.3f, 0.5f);
            }
        });

        // Pequena pausa para respiro visual após a contagem dos pontos
        s.AppendInterval(0.2f);

        // E1. Botão "Try Again" surge primeiro (Scale UP + Punch)
        s.Append(playAgainButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        s.Append(playAgainButton.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.35f, 6, 0.5f));

        // E2. Botão "Leaderboard" surge sequencialmente logo depois (Scale UP + Punch)
        s.Append(leaderboardButton.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack));
        s.Append(leaderboardButton.transform.DOPunchScale(new Vector3(0.25f, 0.25f, 0f), 0.35f, 6, 0.5f));
    }

    private Color SetAlpha(Color c, float alpha)
    {
        c.a = alpha;
        return c;
    }
}