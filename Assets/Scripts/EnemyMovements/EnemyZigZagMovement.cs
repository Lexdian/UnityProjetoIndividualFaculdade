using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/ZigZag")]
public class EnemyZigZagMovement : AbstractEnemyMovement
{
    [Header("Configurações do Zig-Zag")]
    public float frequencia = 7f;  // Velocidade da oscilação lateral
    public float amplitude = 2.5f; // Largura do desvio no eixo X

    public override Vector2 GetDirection(Transform transform, float timeAlive)
    {
        float x = Mathf.Cos(timeAlive * frequencia) * amplitude;
        return new Vector2(x, -1f).normalized;
    }
}