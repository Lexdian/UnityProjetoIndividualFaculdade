using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementsMenu : MonoBehaviour
{

    [Header("Painel de Detalhes da Conquista")]
    [SerializeField] private TMP_Text detailTitleText;
    [SerializeField] private TMP_Text detailDescriptionText;

    [Header("UI Containers & Prefabs")]
    [SerializeField] private RectTransform listContentContainer;
    [SerializeField] private GameObject achievementItemPrefab;

    async void Start()
    {
        // Aguarda até que o AchievementManager e a autenticação do UGS estejam prontos
        while (AchievementManager.Instance == null)
        {
            await Task.Yield();
        }

        GerarMenuConquistas();
    }

    public void GerarMenuConquistas()
    {
        foreach (Transform child in listContentContainer)
        {
            Destroy(child.gameObject);
        }

        var achievements = AchievementManager.Instance.GetAchievements();

        Button uiButton = null;

        foreach (var achievement in achievements)
        {
            GameObject newItem = Instantiate(achievementItemPrefab, listContentContainer);

            AchievementUIItem uiItem = newItem.GetComponent<AchievementUIItem>();
            if (uiItem != null)
            {
                // Passa os textos de destino do painel principal para cada card
                uiItem.Setup(achievement, detailTitleText, detailDescriptionText);
                if (uiButton == null)
                {
                    uiButton = newItem.GetComponent<Button>();
                }
            }
        }
        if (uiButton != null)
        {
            uiButton.Select();
        }
    }
}