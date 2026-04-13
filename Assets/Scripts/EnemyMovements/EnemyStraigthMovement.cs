using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Straight")]
public class EnemyStraigthMovement : AbstractEnemyMovement
{
    public Vector2 direction = Vector2.down;

    public override Vector2 GetDirection()
    {
        return direction.normalized;
    }
}
