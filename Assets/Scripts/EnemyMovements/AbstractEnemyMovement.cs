using UnityEngine;

public abstract class AbstractEnemyMovement : ScriptableObject
{
    public abstract Vector2 GetDirection();
}