using UnityEngine;

[System.Serializable]
public class Achievement
{
    public string id;
    public string title;
    public string description;
    public bool isUnlocked;

    [Header("Progresso (Opcional)")]
    public bool isProgressive;
    public int currentAmount;
    public int targetAmount;
}