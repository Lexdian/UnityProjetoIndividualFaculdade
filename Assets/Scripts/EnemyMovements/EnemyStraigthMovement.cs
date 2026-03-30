using UnityEngine;

[CreateAssetMenu(menuName = "Enemy/Movement/Straight")]
public class EnemyStraigthMovement : AbstractEnemyMovement
{
    public float velocidadeX;
    public float velocidadeY;

    public override void Move(Rigidbody2D rb)
    {
       rb.linearVelocity = new Vector2(velocidadeX, velocidadeY);
    }
}
