using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;
using Unity.Services.Leaderboards.Models;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Leaderboard : MonoBehaviour
{
    private string leaderboardId = "InfinityScore";

    [Header("UI References")]
    [SerializeField] private RectTransform listContentContainer;
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private CanvasGroup contentCanvasGroup;
    [SerializeField] private Image loadingSpinner;

    [Header("Player Rank References")]
    [SerializeField] private List<Sprite> rankIcons;
    [SerializeField] private Image PlayerRankIcon;
    [SerializeField] private TMP_Text playerScore;

    [Header("Configuração de Loading Animado")]
    [SerializeField] private Sprite[] loadingImages;
    [SerializeField] private float frameRate = 0.15f;
    private Coroutine loadingAnimationCoroutine;
    private bool carregamentoConcluido = false;

    async void Start()
    {
        PrepararUIParaCarregamento();

        while (UGSManager.Instance == null ||
               !UGSManager.Instance.IsReady ||
               !AuthenticationService.Instance.IsSignedIn)
        {
            await Task.Yield();
        }

        await AtualizarUIScoreboard();
    }

    private void PrepararUIParaCarregamento()
    {
        carregamentoConcluido = false;

        if (contentCanvasGroup != null)
        {
            contentCanvasGroup.alpha = 0f;
        }

        if (loadingSpinner != null)
        {
            loadingSpinner.gameObject.SetActive(true);
            loadingSpinner.transform.localScale = Vector3.one;

            if (loadingAnimationCoroutine != null)
            {
                StopCoroutine(loadingAnimationCoroutine);
            }
            loadingAnimationCoroutine = StartCoroutine(AnimarLoadingCiclico());
        }

        if (PlayerRankIcon != null) PlayerRankIcon.enabled = false;
        if (playerScore != null) playerScore.text = "---";
    }

    private IEnumerator AnimarLoadingCiclico()
    {
        if (loadingImages == null || loadingImages.Length == 0) yield break;

        int currentFrame = 0;
        WaitForSeconds waitTime = new WaitForSeconds(frameRate);

        while (true)
        {
            loadingSpinner.sprite = loadingImages[currentFrame];

            // Se os dados já chegaram e estamos no último frame da animação:
            if (carregamentoConcluido && currentFrame == loadingImages.Length - 1)
            {
                FinalizarTransicaoUI();
                yield break; // Interrompe o loop
            }

            currentFrame = (currentFrame + 1) % loadingImages.Length;
            yield return waitTime;
        }
    }

    public async Task AtualizarUIScoreboard()
    {
        foreach (Transform child in listContentContainer)
        {
            Destroy(child.gameObject);
        }

        try
        {
            var scoresTask = LeaderboardsService.Instance.GetScoresAsync(
                leaderboardId,
                new GetScoresOptions { Limit = 100 }
            );

            var playerScoreTask = LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);

            await Task.WhenAll(scoresTask, playerScoreTask);

            var scores = scoresTask.Result;
            var playerScoreResponse = playerScoreTask.Result;

            foreach (var entry in scores.Results)
            {
                GameObject newLine = Instantiate(linePrefab, listContentContainer);

                LeaderboardEntry entryComponent = newLine.GetComponent<LeaderboardEntry>();
                TMP_Text rankText = entryComponent.Rank;
                TMP_Text nameText = entryComponent.Name;
                TMP_Text scoreText = entryComponent.Score;
                Image rankIcon = entryComponent.MedalIcon;

                int pos = entry.Rank + 1;
                string suffix = pos == 1 ? "st" : pos == 2 ? "nd" : pos == 3 ? "rd" : "th";

                rankText.text = pos + suffix;
                nameText.text = string.IsNullOrEmpty(entry.PlayerName)
                    ? entry.PlayerId.Substring(0, 6)
                    : entry.PlayerName;

                scoreText.text = entry.Score.ToString("N0");

                string tierId = entry.Tier ?? "";
                Sprite medalSprite = GetMedalhaSprite(tierId);

                if (medalSprite != null)
                {
                    rankIcon.sprite = medalSprite;
                    rankIcon.enabled = true;
                }
                else
                {
                    rankIcon.enabled = false;
                }
            }

            playerScore.text = playerScoreResponse.Score.ToString("N0");

            Sprite playerMedal = GetMedalhaSprite(playerScoreResponse.Tier ?? "");
            if (playerMedal != null)
            {
                PlayerRankIcon.sprite = playerMedal;
                PlayerRankIcon.enabled = true;
            }

            // Sinaliza para a Coroutine que os dados estão prontos
            carregamentoConcluido = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao carregar Leaderboard: " + e.Message);
            // Em caso de erro, encerra imediatamente para não travar a UI
            carregamentoConcluido = true;
            FinalizarTransicaoUI();
        }
    }

    private void FinalizarTransicaoUI()
    {
        if (loadingSpinner != null)
        {
            loadingSpinner.transform.DOKill();
            loadingSpinner.transform.DOScale(Vector3.zero, 0.2f).OnComplete(() =>
            {
                loadingSpinner.gameObject.SetActive(false);
            });
        }

        if (contentCanvasGroup != null)
        {
            contentCanvasGroup.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
        }
    }

    private Sprite GetMedalhaSprite(string tierId)
    {
        switch (tierId)
        {
            case "Sucata": return rankIcons.Count > 0 ? rankIcons[0] : null;
            case "Sobrevivente": return rankIcons.Count > 1 ? rankIcons[1] : null;
            case "Dancarino": return rankIcons.Count > 2 ? rankIcons[2] : null;
            case "Destruidor": return rankIcons.Count > 3 ? rankIcons[3] : null;
            case "Mestre": return rankIcons.Count > 4 ? rankIcons[4] : null;
            default: return null;
        }
    }
}