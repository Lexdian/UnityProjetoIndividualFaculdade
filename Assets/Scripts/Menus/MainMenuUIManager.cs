using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using UnityEngine;
using UnityEngine.UI;

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
        // 1. Aguarda inicialização do UGS
        while (UGSManager.Instance == null || !UGSManager.Instance.IsReady)
            await Task.Yield();

        // 2. Busca primeiro se o jogador JÁ salvou um nome manualmente no Cloud Save
        string savedName = await LoadNameFromCloud();

        // 3. Fallback: Se o Cloud Save falhar no WebGL, checa no PlayerPrefs local
        if (string.IsNullOrEmpty(savedName))
        {
            savedName = PlayerPrefs.GetString(NAME_KEY, "");
        }

        // 4. Se encontrou um nome salvo pelo jogador, entra direto no Menu
        if (!string.IsNullOrEmpty(savedName))
        {
            nomeJogador.text = "Username: " + savedName;
            panelName.SetActive(false);

            _ = ShowAndAnimateButtonsAsync();
        }
        else
        {
            // Primeira vez abrindo o jogo: Força o painel de escolha de nome
            panelPrincipal.SetActive(false);
            panelName.SetActive(true);
        }
    }

    public async void SetName()
    {
        if (string.IsNullOrWhiteSpace(inputName.text))
            return;

        string newName = inputName.text;

        // 1. Envia para o UGS e pega o nome formatado com o # numérico do servidor
        string officialUgsName = await UGSManager.Instance.SetName(newName);
        string nameToDisplay = !string.IsNullOrEmpty(officialUgsName) ? officialUgsName : newName;

        // 2. Salva a escolha tanto no Cloud Save quanto no PlayerPrefs local do navegador
        await SaveNameToCloud(nameToDisplay);
        PlayerPrefs.SetString(NAME_KEY, nameToDisplay);
        PlayerPrefs.Save();

        // 3. Atualiza a UI e mostra os botões do menu
        nomeJogador.text = "Username: " + nameToDisplay;
        panelName.SetActive(false);

        _ = ShowAndAnimateButtonsAsync();
    }

    private async Task ShowAndAnimateButtonsAsync()
    {
        RectTransform[] buttons = new RectTransform[] { button1, button2, button3, button4 };

        buttonsCanvasGroup.alpha = 0f;
        panelPrincipal.SetActive(true);

        VerticalLayoutGroup layoutGroup = panelPrincipal.GetComponentInChildren<VerticalLayoutGroup>();
        if (layoutGroup != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
        }
        Canvas.ForceUpdateCanvases();

        Vector2[] targetPositions = new Vector2[buttons.Length];
        Vector2[] startPositions = new Vector2[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;

            targetPositions[i] = buttons[i].anchoredPosition;
            startPositions[i] = new Vector2(targetPositions[i].x - offscreenOffset, targetPositions[i].y);
        }

        if (layoutGroup != null)
        {
            layoutGroup.enabled = false;
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] == null) continue;
            buttons[i].anchoredPosition = startPositions[i];
        }

        buttonsCanvasGroup.alpha = 1f;

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