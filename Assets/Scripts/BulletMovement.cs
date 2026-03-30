using UnityEngine;

public class BulletMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float velocidadeY = 1f;
    Rigidbody2D rb;

    public float TimeToDeath = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Destroy(this, TimeToDeath);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(0, velocidadeY);
    }
}
