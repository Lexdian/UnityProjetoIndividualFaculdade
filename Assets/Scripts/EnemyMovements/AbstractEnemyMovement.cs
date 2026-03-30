using UnityEngine;

public abstract class AbstractEnemyMovement : ScriptableObject
{
    public abstract void Move(Rigidbody2D rb);
}