using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Importante para o ISelectHandler

public class AchievementUIItem : MonoBehaviour, ISelectHandler
{
    [Header("UI References (Do Card)")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image containerBackground;

    [Header("UI References (Painel de Detalhes na Tela)")]
    [SerializeField] private TMP_Text detailTitleText;       // Objeto de texto do Título na tela
    [SerializeField] private TMP_Text detailDescriptionText; // Objeto de texto da Descrição na tela

    [Header("Cores do Status")]
    [SerializeField] private Color colorLocked = new Color(0.75f, 0.75f, 0.75f, 1f);   // Prata / Cinza
    [SerializeField] private Color colorUnlocked = new Color(1f, 0.84f, 0f, 1f);      // Dourado

    private Achievement currentAchievement;

    public void Setup(
        Achievement achievement,
        TMP_Text externalTitleText = null,
        TMP_Text externalDescriptionText = null)
    {
        currentAchievement = achievement;

        // Se passarmos as referências no Setup, atualiza as variáveis de detalhes
        if (externalTitleText != null) detailTitleText = externalTitleText;
        if (externalDescriptionText != null) detailDescriptionText = externalDescriptionText;

        if (titleText != null) titleText.text = achievement.title;

        // Aplica a cor de status (Dourado se concluído, Prata se bloqueado)
        if (containerBackground != null)
        {
            containerBackground.color = achievement.isUnlocked ? colorUnlocked : colorLocked;
        }
    }

    // Chamado automaticamente pelo EventSystem assim que o item é SELECIONADO (foco via Gamepad, Teclado ou Clique)
    public void OnSelect(BaseEventData eventData)
    {
        ShowDetails();
    }

    private void ShowDetails()
    {
        if (currentAchievement == null) return;

        // Atualiza o Título no painel de detalhes
        if (detailTitleText != null)
        {
            detailTitleText.text = currentAchievement.title;
        }

        // Formata e atualiza a Descrição no painel de detalhes
        if (detailDescriptionText != null)
        {
            string fullDescription = currentAchievement.description;

            if(currentAchievement.isUnlocked)
            {
                fullDescription += "\n\nCompleted!";
            }
            else if (currentAchievement.isProgressive)
            {
                fullDescription += $"\n\nProgress: {currentAchievement.currentAmount}/{currentAchievement.targetAmount}";
            }

            detailDescriptionText.text = fullDescription;
        }
    }
}