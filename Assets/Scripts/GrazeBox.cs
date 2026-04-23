using UnityEngine;

public class GrazeBox : MonoBehaviour
{
    private PlayerMovement playerScript;
    private GrazeVisual visual; // Referência ao novo script

    void Start()
    {
        playerScript = GetComponentInParent<PlayerMovement>();
        visual = GetComponent<GrazeVisual>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyBullet"))
        {
            playerScript.RegistrarGraze();

            // Ativa o círculo visual
            if (visual != null) visual.MostrarFeedback();
        }
    }
}