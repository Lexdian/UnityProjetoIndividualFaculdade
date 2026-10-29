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

        try
        {
            var playerScore = await LeaderboardsService.Instance.GetPlayerScoreAsync(leaderboardId);
            if (GameManager.Instance != null)
            {
                GameManager.Instance.HighScore = playerScore != null ? (int)playerScore.Score : 0;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Aviso ao obter HighScore inicial: {e.Message}");
        }
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

    /// <summary>
    /// Atualiza o nome do jogador no UGS e retorna a string oficial formatada com #numeração
    /// </summary>
    public async Task<string> SetName(string nome)
    {
        try
        {
            string nomeOficial = await AuthenticationService.Instance.UpdatePlayerNameAsync(nome);
            Debug.Log("Nome atualizado no UGS: " + nomeOficial);
            return nomeOficial;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao definir nome no UGS: " + e.Message);
            return nome;
        }
    }

    /// <summary>
    /// Requisita ao servidor do UGS o nome oficial do jogador formatado com o sufixo (#XXXX)
    /// </summary>
    public async Task<string> GetName()
    {
        try
        {
            string playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            Debug.Log("Nome do jogador obtido do UGS: " + playerName);
            return playerName;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Erro ao obter nome do UGS: " + e.Message);
            return AuthenticationService.Instance.PlayerName;
        }
    }
}