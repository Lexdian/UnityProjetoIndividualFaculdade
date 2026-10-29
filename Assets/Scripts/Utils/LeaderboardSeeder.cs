using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Leaderboards;

public class LeaderboardSeeder : MonoBehaviour
{
    [Header("Configurações do Leaderboard")]
    [SerializeField] private string leaderboardId = "InfinityScore";
    [SerializeField] private int quantidadeJogadores = 100;
    [SerializeField] private int pontuacaoMinima = 1000;
    [SerializeField] private int pontuacaoMaxima = 1000000;

    [Header("Atraso entre requisições (Evita Rate Limit)")]
    [SerializeField] private int delayMsEntreEnvios = 150;

    private readonly string[] prefixosNomes = new string[]
    {
        "Ace", "Viper", "Cyber", "Nova", "Phantom", "Rogue", "Shadow", "Titan", "Blaze",
        "Apex", "Vector", "Cosmo", "Astra", "Echo", "Falcon", "Ghost", "Zenith", "Omega"
    };

    [ContextMenu("Gerar 100 Scores Fakes")]
    public async void PopularLeaderboard()
    {
        Debug.Log("Iniciando geração de 100 pontuações fakes no UGS...");

        // Garante que o Unity Services está inicializado
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        // Desconecta o perfil atual para não sobrescrever a conta principal do desenvolvedor
        if (AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
        }

        List<int> pontuacoes = GerarPontuacoesDistribuidas(quantidadeJogadores, pontuacaoMinima, pontuacaoMaxima);

        for (int i = 0; i < quantidadeJogadores; i++)
        {
            string profileName = $"FakeProfile_{i + 1}";
            string nickName = $"{prefixosNomes[UnityEngine.Random.Range(0, prefixosNomes.Length)]}_{UnityEngine.Random.Range(100, 999)}";
            int score = pontuacoes[i];

            try
            {
                // 1. Troca o perfil do dispositivo para criar/acessar um jogador fake único
                AuthenticationService.Instance.SwitchProfile(profileName);

                if (!AuthenticationService.Instance.IsSignedIn)
                {
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();
                }

                // 2. Define o nome do jogador no UGS
                await AuthenticationService.Instance.UpdatePlayerNameAsync(nickName);

                // 3. Envia a pontuação para o Leaderboard
                await LeaderboardsService.Instance.AddPlayerScoreAsync(leaderboardId, score);

                Debug.Log($"[{i + 1}/{quantidadeJogadores}] Cadastrado: {nickName} | Score: {score:N0}");

                // Desconecta para o próximo ciclo
                AuthenticationService.Instance.SignOut();

                // Pequena pausa para evitar bloqueio por excesso de requisições por segundo (Rate Limit)
                await Task.Delay(delayMsEntreEnvios);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Erro ao enviar fake #{i + 1}: {ex.Message}");
            }
        }

        Debug.Log("Processo concluído! Restaurando perfil padrão...");

        // Restaura a sessão do perfil padrão
        RestaurarSessaoPadrao();
    }

    /// <summary>
    /// Gera pontuações curva exponencial (mais jogadores em pontuações médias/baixas e poucos no topo)
    /// </summary>
    private List<int> GerarPontuacoesDistribuidas(int quantidade, int min, int max)
    {
        List<int> lista = new List<int>();

        for (int i = 0; i < quantidade; i++)
        {
            // Curva de distribuição para criar um placar realista estilo arcade
            float t = (float)i / (quantidade - 1);
            float curva = Mathf.Pow(t, 2.2f); // Exponencial

            int score = Mathf.RoundToInt(Mathf.Lerp(min, max, curva));

            // Arredonda para múltiplos de 10
            score = (score / 10) * 10;

            lista.Add(score);
        }

        // Embaralha para não enviar em ordem estritamente crescente
        for (int i = 0; i < lista.Count; i++)
        {
            int temp = lista[i];
            int randomIndex = UnityEngine.Random.Range(i, lista.Count);
            lista[i] = lista[randomIndex];
            lista[randomIndex] = temp;
        }

        return lista;
    }

    private async void RestaurarSessaoPadrao()
    {
        try
        {
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }

            AuthenticationService.Instance.SwitchProfile("default");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("Sessão padrão restaurada com sucesso. PlayerId: " + AuthenticationService.Instance.PlayerId);
        }
        catch (Exception ex)
        {
            Debug.LogError("Erro ao restaurar perfil padrão: " + ex.Message);
        }
    }
}