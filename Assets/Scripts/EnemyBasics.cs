using Unity.VisualScripting;
using UnityEngine;

public class EnemyBasics : MonoBehaviour
{
    public int Health = 1;

    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Bullet")) {
            Health--;
            if (Health <= 0) {
                Destroy(collision.gameObject);
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log("Collided with something that is not a bullet: " + collision.gameObject.name);
        }
    }
}
