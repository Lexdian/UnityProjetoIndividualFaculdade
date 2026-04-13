using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f;

    Rigidbody2D rb;
    public AbstractEnemyMovement movementPattern;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        Vector2 dir = movementPattern.GetDirection();
        rb.linearVelocity = dir * speed;

        if (transform.position.y < -6f)
        {
            gameObject.SetActive(false);
        }
    }

    public void Setup(AbstractEnemyMovement pattern)
    {
        movementPattern = pattern;
    }
}