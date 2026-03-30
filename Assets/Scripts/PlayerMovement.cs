using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    float moveHorizontal;
    float moveVertical;
    public float velocidade = 1f;
    public GameObject bulletPrefab;
    Rigidbody2D rb;
    public Vector2 bulletSpawnOffset = new Vector2(0, 1f);
    public float fireRate = 0.5f; // Tempo entre tiros em segundos
    public float nextFireTime = 0f; // Tempo em que o próximo tiro pode ser disparado
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Moved(InputAction.CallbackContext context) { 
        moveHorizontal = context.ReadValue<Vector2>().x;
        moveVertical = context.ReadValue<Vector2>().y;
    }
    public void Shoot(InputAction.CallbackContext context) { 
        if (context.performed && nextFireTime>fireRate) {
            GameObject bullet = Instantiate(bulletPrefab, transform.position + (Vector3)bulletSpawnOffset, bulletPrefab.transform.rotation);
            nextFireTime = 0;
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveHorizontal, moveVertical) * velocidade;
        nextFireTime += Time.fixedDeltaTime;
    }
}
