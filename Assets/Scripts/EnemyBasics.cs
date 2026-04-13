using UnityEngine;

public class EnemyBasics : MonoBehaviour
{
    public int Health = 1;

    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Health--;

            if (anim != null)
            {
                anim.SetTrigger("Damaged");
            }

            Destroy(collision.gameObject);

            if (Health <= 0)
            {
                gameObject.SetActive(false);
            }
        }
    }
}