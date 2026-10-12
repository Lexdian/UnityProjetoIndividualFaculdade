using UnityEngine;
using System;

[Serializable]
public class ShootingPattern
{
    public bool aimAtPlayer = true; // Opcional: Atirar na direção do jogador ou não
    public int directions = 1;
    public int burst = 1;
    public float fireRate = 0.2f;
    public float rotationSpeed = 30f;
    public float bulletSpeed = 5f;
    public float angleStep = 0;
    public float angleSpread = 0f;
    public float speedIncrement = 0f;

    [Header("Configuração de Salva (Volley)")]
    public int shotsPerVolley = 5;
    public float timeBetweenVolleys = 2f;
}

public class EnemyDifficultySettings : MonoBehaviour
{
    public ShootingPattern facil;
    public ShootingPattern medio;
    public ShootingPattern dificil;
    public ShootingPattern mutante;

    public ShootingPattern GetPattern(int level)
    {
        if (level <= 1) return facil;
        if (level == 2) return medio;
        return dificil;
    }
}