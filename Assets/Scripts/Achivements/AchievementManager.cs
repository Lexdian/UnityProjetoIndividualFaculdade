using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using DeleteOptions = Unity.Services.CloudSave.Models.Data.Player.DeleteOptions;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<Achievement> achievements = new List<Achievement>();

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await InitializeAndLoad();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async Task InitializeAndLoad()
    {
        PopulateDefaultAchievements();

        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await UnityServices.InitializeAsync();
        }

        while (!AuthenticationService.Instance.IsSignedIn)
        {
            await Task.Yield();
        }

        await LoadAchievementsFromCloud();
    }

    private void PopulateDefaultAchievements()
    {
        if (achievements.Count > 0) return;

        achievements = new List<Achievement>
        {
            // ==========================================
            // SCORE ACHIEVEMENTS (SINGLE RUN)
            // ==========================================
            new Achievement
            {
                id = "score",
                title = "Baptism of Fire",
                description = "Score 10,000 points in a single run.",
                isProgressive = false,
                targetAmount = 10000
            },
            new Achievement
            {
                id = "score",
                title = "Aces High",
                description = "Score 50,000 points in a single run.",
                isProgressive = false,
                targetAmount = 50000
            },
            new Achievement
            {
                id = "score",
                title = "Cosmobreaker",
                description = "Score 100,000 points in a single run.",
                isProgressive = false,
                targetAmount = 100000
            },
            new Achievement
            {
                id = "score",
                title = "Star Destroyer",
                description = "Score 250,000 points in a single run.",
                isProgressive = false,
                targetAmount = 250000
            },
            new Achievement
            {
                id = "score",
                title = "Galactic Overlord",
                description = "Score 500,000 points in a single run.",
                isProgressive = false,
                targetAmount = 500000
            },
            new Achievement
            {
                id = "score",
                title = "Bullet Hell Messiah",
                description = "Score 1,000,000 points in a single run.",
                isProgressive = false,
                targetAmount = 1000000
            },

            // ==========================================
            // ENEMY KILL ACHIEVEMENTS (INDIVIDUALS)
            // ==========================================
            new Achievement
            {
                id = "elite_enemy_kill",
                title = "Elite Hunter",
                description = "Eliminate 50 Elite class enemies.",
                isProgressive = true,
                targetAmount = 50
            },
            new Achievement
            {
                id = "elite_enemy_kill",
                title = "Apex Predator",
                description = "Eliminate 250 Elite class enemies.",
                isProgressive = true,
                targetAmount = 250
            },
            new Achievement
            {
                id = "giant_enemy_kill",
                title = "The Bigger They Are...",
                description = "Destroy 5 Giant enemies.",
                isProgressive = true,
                targetAmount = 5
            },
            new Achievement
            {
                id = "giant_enemy_kill",
                title = "Colossus Slayer",
                description = "Destroy 25 Giant enemies.",
                isProgressive = true,
                targetAmount = 25
            },
            new Achievement
            {
                id = "elite_giant_enemy_kill",
                title = "Titan Slayer",
                description = "Eliminate 10 Giant Elite enemies.",
                isProgressive = true,
                targetAmount = 10
            },
            new Achievement
            {
                id = "accelerated_enemy_kill",
                title = "Quick Draw",
                description = "Take down 25 Accelerated enemies.",
                isProgressive = true,
                targetAmount = 25
            },
            new Achievement
            {
                id = "accelerated_enemy_kill",
                title = "Steel Reflexes",
                description = "Take down 50 Accelerated enemies.",
                isProgressive = true,
                targetAmount = 50
            },
            new Achievement
            {
                id = "accelerated_elite_enemy_kill",
                title = "Terminal Velocity",
                description = "Eliminate 15 Accelerated Elite enemies.",
                isProgressive = true,
                targetAmount = 15
            },
            new Achievement
            {
                id = "mutant_enemy_kill",
                title = "Biohazard Threat",
                description = "Eradicate 25 Mutant enemies.",
                isProgressive = true,
                targetAmount = 25
            },
            new Achievement
            {
                id = "mutant_enemy_kill",
                title = "Pest Control",
                description = "Eradicate 50 Mutant enemies.",
                isProgressive = true,
                targetAmount = 50
            },
            new Achievement
            {
                id = "mutant_elite_enemy_kill",
                title = "Super Anomaly",
                description = "Eliminate 15 Mutant Elite enemies.",
                isProgressive = true,
                targetAmount = 15
            },

            // ==========================================
            // DUAL COMBINATION ACHIEVEMENTS
            // ==========================================
            new Achievement
            {
                id = "giant_accelerated_enemy_kill",
                title = "Fast & Furious Colossus",
                description = "Destroy 3 Giant Accelerated enemies.",
                isProgressive = true,
                targetAmount = 3
            },
            new Achievement
            {
                id = "giant_accelerated_elite_enemy_kill",
                title = "Hyper Titan Hunter",
                description = "Eliminate 2 Giant Accelerated Elite enemies.",
                isProgressive = true,
                targetAmount = 2
            },
            new Achievement
            {
                id = "giant_mutant_enemy_kill",
                title = "Toxic Heavyweight",
                description = "Destroy 3 Giant Mutant enemies.",
                isProgressive = true,
                targetAmount = 3
            },
            new Achievement
            {
                id = "giant_mutant_elite_enemy_kill",
                title = "Bio-Colossus Extinction",
                description = "Eliminate 2 Giant Mutant Elite enemies.",
                isProgressive = true,
                targetAmount = 2
            },
            new Achievement
            {
                id = "accelerated_mutant_enemy_kill",
                title = "Radioactive Streak",
                description = "Destroy 5 Accelerated Mutant enemies.",
                isProgressive = true,
                targetAmount = 5
            },
            new Achievement
            {
                id = "accelerated_mutant_elite_enemy_kill",
                title = "Furious Contagion",
                description = "Eliminate 3 Accelerated Mutant Elite enemies.",
                isProgressive = true,
                targetAmount = 3
            },

            // ==========================================
            // TRIPLE COMBINATION ACHIEVEMENTS
            // ==========================================
            new Achievement
            {
                id = "triple_threat_enemy_kill",
                title = "Unstoppable Anomaly",
                description = "Destroy a Giant Accelerated Mutant enemy.",
                isProgressive = true,
                targetAmount = 1
            },
            new Achievement
            {
                id = "triple_threat_elite_enemy_kill",
                title = "Giga Overlord Slayer",
                description = "Eliminate a Giant Accelerated Mutant Elite enemy.",
                isProgressive = true,
                targetAmount = 1
            },

            // ==========================================
            // CUMULATIVE & LIFE ACHIEVEMENTS
            // ==========================================
            new Achievement
            {
                id = "enemy_kill",
                title = "Scrap Metal",
                description = "Destroy a total of 1,000 enemies.",
                isProgressive = true,
                targetAmount = 1000
            },
            new Achievement
            {
                id = "enemy_kill",
                title = "Armageddon",
                description = "Destroy a total of 5,000 enemies.",
                isProgressive = true,
                targetAmount = 5000
            }
        };
    }

    private string GetSanitizedKey(string title, string suffix)
    {
        string cleanTitle = Regex.Replace(title, @"[^a-zA-Z0-9_]", "_").ToLower();
        return $"ach_{cleanTitle}_{suffix}";
    }

    public async void UnlockAchievement(string idOrTitle)
    {
        List<Achievement> matching = achievements.FindAll(a => a.id == idOrTitle || a.title == idOrTitle);
        foreach (var achievement in matching)
        {
            if (!achievement.isUnlocked)
            {
                await CompleteAchievement(achievement);
            }
        }
    }

    public async void UnlockAchievementsByTarget(string idPrefix, int amount)
    {
        List<Achievement> matchingAchievements = achievements.FindAll(a => a.id.StartsWith(idPrefix));

        foreach (var achievement in matchingAchievements)
        {
            if (!achievement.isUnlocked && achievement.targetAmount <= amount && !achievement.isProgressive)
            {
                await CompleteAchievement(achievement);
            }
        }
    }

    public List<Achievement> GetAchievements()
    {
        return achievements;
    }

    public async void AddProgress(string idOrTitle, int amount = 1)
    {
        List<Achievement> matching = achievements.FindAll(a => a.id == idOrTitle || a.title == idOrTitle);

        foreach (var achievement in matching)
        {
            if (!achievement.isUnlocked && achievement.isProgressive)
            {
                achievement.currentAmount += amount;

                if (achievement.currentAmount >= achievement.targetAmount)
                {
                    await CompleteAchievement(achievement);
                }
                else
                {
                    await SaveProgressToCloud(achievement);
                }
            }
        }
    }

    private async Task CompleteAchievement(Achievement achievement)
    {
        achievement.isUnlocked = true;

        string unlockKey = GetSanitizedKey(achievement.title, "unlocked");
        string progressKey = GetSanitizedKey(achievement.title, "progress");

        var dataToSave = new Dictionary<string, object>
        {
            { unlockKey, true },
            { progressKey, achievement.currentAmount }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);

        Debug.Log($"Achievement Unlocked: {achievement.title}");
    }

    private async Task SaveProgressToCloud(Achievement achievement)
    {
        string progressKey = GetSanitizedKey(achievement.title, "progress");

        var dataToSave = new Dictionary<string, object>
        {
            { progressKey, achievement.currentAmount }
        };

        await CloudSaveService.Instance.Data.Player.SaveAsync(dataToSave);
    }

    private async Task LoadAchievementsFromCloud()
    {
        try
        {
            var savedData = await CloudSaveService.Instance.Data.Player.LoadAllAsync();

            foreach (var achievement in achievements)
            {
                string unlockKey = GetSanitizedKey(achievement.title, "unlocked");
                string progressKey = GetSanitizedKey(achievement.title, "progress");

                if (savedData.TryGetValue(unlockKey, out var unlockItem))
                {
                    achievement.isUnlocked = unlockItem.Value.GetAs<bool>();
                }

                if (savedData.TryGetValue(progressKey, out var progressItem))
                {
                    achievement.currentAmount = progressItem.Value.GetAs<int>();
                }
            }

            Debug.Log("Achievements loaded from cloud successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading achievements from Cloud Save: {e.Message}");
        }
    }

    [ContextMenu("Limpar Todas as Conquistas")]
    public async void ClearAllAchievements()
    {
        if (achievements == null || achievements.Count == 0)
        {
            PopulateDefaultAchievements();
        }

        foreach (var achievement in achievements)
        {
            achievement.isUnlocked = false;
            achievement.currentAmount = 0;
        }

        try
        {
            if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
            {
                foreach (var achievement in achievements)
                {
                    string unlockKey = GetSanitizedKey(achievement.title, "unlocked");
                    string progressKey = GetSanitizedKey(achievement.title, "progress");

                    try
                    {
                        await CloudSaveService.Instance.Data.Player.DeleteAsync(unlockKey, new DeleteOptions());
                        await CloudSaveService.Instance.Data.Player.DeleteAsync(progressKey, new DeleteOptions());
                    }
                    catch
                    {
                        // Se a chave não existir na nuvem, apenas continua
                    }
                }
                Debug.Log("Todas as conquistas foram limpas no Cloud Save e localmente.");
            }
            else
            {
                Debug.LogWarning("Conquistas resetadas apenas localmente (Usuário não está autenticado no UGS).");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erro ao apagar dados do Cloud Save: {e.Message}");
        }
    }
}

// ==========================================
// INSPECTOR CUSTOMIZADO PARA EXIBIR O BOTÃO
// ==========================================
#if UNITY_EDITOR
[CustomEditor(typeof(AchievementManager))]
public class AchievementManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        AchievementManager manager = (AchievementManager)target;

        GUILayout.Space(15);
        GUI.backgroundColor = new Color(1f, 0.3f, 0.3f);

        if (GUILayout.Button("Limpar Todas as Conquistas (Local e Cloud)", GUILayout.Height(35)))
        {
            if (EditorUtility.DisplayDialog("Confirmar Reset",
                "Tem certeza que deseja apagar todo o progresso das conquistas no dispositivo e no Cloud Save?",
                "Sim, Limpar", "Cancelar"))
            {
                manager.ClearAllAchievements();
            }
        }

        GUI.backgroundColor = Color.white;
    }
}
#endif