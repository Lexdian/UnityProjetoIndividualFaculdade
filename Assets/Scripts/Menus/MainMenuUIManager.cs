using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.CloudSave;

public class MainMenuUIManager : MonoBehaviour
{
    [Header("Painéis")]
    public GameObject panelPrincipal;
    public GameObject panelName;

    [Header("UI Text / Input")]
    public TMP_InputField inputName;
    public TMP_Text nomeJogador;

    [Header("Botões do Menu")]
    public CanvasGroup buttonsCanvasGroup;
    public RectTransform button1;
    public RectTransform button2;
    public RectTransform button3;
    public RectTransform button4;

    [Header("Configurações de Animação")]
    [SerializeField] private float animationDuration = 0.35f;
    [SerializeField] private float staggerDelay = 0.1f;
    [SerializeField] private float offscreenOffset = 1000f;

    private const string NAME_KEY = "player_name";

    private async void Start()
    {
        // Aguarda a inicialização e autenticação do UGS
        while (!UGSManager.Instance.IsReady)
            await Task.Yield();

        // Carrega o nome salvo diretamente do Cloud Save
        string cloudName = await LoadNameFromCloud();

        if (!string.IsNullOrEmpty(cloudName))
        {
            nomeJogador.text = "Username: " + cloudName;
            panelName.SetActive(false);

            // Inicia a exibição e animação sem "flicker"
            _ = ShowAndAnimateButtonsAsync();
        }
        else
        {
            panelPrincipal.SetActive(false);
            panelName.SetActive(true);
        }
    }

    public async void SetName()
    {
        if (string.IsNullOrWhiteSpace(inputName.text))
            return;

        string newName = inputName.text;

        // Salva no UGS/Leaderboard
        await UGSManager.Instance.SetName(newName);

        // Salva a chave no Unity Cloud Save
        await SaveNameToCloud(newName);

        nomeJogador.text = "Username: " + newName;
        panelName.SetActive(false);

        // Inicia a exibição e animação sem "flicker"
        _ = ShowAndAnimateButtonsAsync();
    }

    private async Task ShowAndAnimateButtonsAsync()
    {
        RectTransform[] buttons = new RectTransform[] { button1, button2, button3, button4 };

        // 1. Oculta o painel visualmente antes de ativá-lo
        buttonsCanvasGroup.alpha = 0f;
        panelPrincipal.SetActive(true);

        // 2. Força o Unity a calcular o layout dos botões instantaneamente em segundo plano
        VerticalLayoutGroup layoutGroup = panelPrincipal.GetComponentInChildren<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
        Canvas.ForceUpdateCanvases();

        // 3. Salva as posições calculadas e desloca os botões para fora da tela
        Vector2[] targetPositions = new Vector2[buttons.Length];
        Vector2[] startPositions = new Vector2[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            targetPositions[i] = buttons[i].anchoredPosition;
            startPositions[i] = new Vector2(targetPositions[i].x - offscreenOffset, targetPositions[i].y);
        }

        // Desativa o LayoutGroup para não travar as posições dos botões
        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
        }

        // Aplica a posição inicial (fora da tela à esquerda)
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            buttons[i].anchoredPosition = startPositions[i];
        }

        // 4. Revela o CanvasGroup agora que os botões já estão posicionados fora da tela
        buttonsCanvasGroup.alpha = 1f;

        // 5. Anima a entrada dos botões um a um
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            float elapsedTime = 0f;
            Vector2 startPos = startPositions[i];
            Vector2 targetPos = targetPositions[i];

            while (elapsedTime < animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / animationDuration);
                t = Mathf.SmoothStep(0f, 1f, t);

                buttons[i].anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                await Task.Yield();
            }

            buttons[i].anchoredPosition = targetPos;

            // Espera o tempo configurado antes de animar o próximo botão
            if (staggerDelay > 0f)
            {
                float delayTimer = 0f;
                while (delayTimer < staggerDelay)
                {
                    delayTimer += Time.deltaTime;
                    await Task.Yield();
                }
            }
        }

        // Reativa o LayoutGroup ao final da animação
        if (layoutGroup != null)
        {
            layoutGroup.enabled = true;
        }
    }

    private async Task SaveNameToCloud(string name)
    {
        try
        {
            var data = new Dictionary<string, object> { { NAME_KEY, name } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data);
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao salvar nome no Cloud Save: {e.Message}");
        }
    }

    private async Task<string> LoadNameFromCloud()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { NAME_KEY });

            if (savedData.TryGetValue(NAME_KEY, out var item))
            {
                return item.Value.GetAs<string>();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao carregar nome do Cloud Save: {e.Message}");
        }

        return null;
    }
}