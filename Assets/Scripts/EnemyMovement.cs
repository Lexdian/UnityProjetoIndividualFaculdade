using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float velocidadeX = 1f;
    public float velocidadeY = 1f;
    Rigidbody2D rb;

    public AbstractEnemyMovement movementPattern;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        movementPattern.Move(rb);
    }
}
