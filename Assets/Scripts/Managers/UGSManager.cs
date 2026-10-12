using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Leaderboards;
using UnityEngine;

public class UGSManager : MonoBehaviour
{
    public static UGSManager Instance { get; private set; }

    private string leaderboardId = "InfinityScore";
    public bool IsReady { get; private set; } = false;
    async void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitUGS();
    }

    async Task InitUGS()
    {
        await UnityServices.InitializeAsync();
        Debug.Log(UnityServices.State);

        await SignUpAnonymouslyAsync();

        IsReady = true;
        var playerScore = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
        GameManager.Instance.HighScore = playerScore != null ? (int)playerScore.Score : 0;
    }

    async Task SignUpAnonymouslyAsync()
    {
        try
        {
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            Debug.Log("Sign in anonymously succeeded!");
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task AddScore(int score)
    {
        try
        {
            await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);
            Debug.Log("Score enviado!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao enviar score: " + e.Message);
        }
    }

    public async Task SetName(string nome)
    {
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(nome);
            Debug.Log("Nome atualizado!");
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao definir nome: " + e.Message);
        }
    }

    public string GetName()
    {
        try
        {
            string playerName = AuthenticationService.Instance.PlayerName;
            Debug.Log("Nome do jogador: " + playerName);
            return playerName;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao obter nome: " + e.Message);
            return null;
        }
    }
}
