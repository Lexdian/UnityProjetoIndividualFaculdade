using UnityEngine;

public class BulletMovementHoming : BulletMovement
{
    [Header("Configurações do Míssil Teleguiado")]
    [Tooltip("Velocidade de curva em graus por segundo (valores menores fazem curvas mais largas/lentas)")]
    [SerializeField] private float rotationSpeed = 120f;

    [Tooltip("Tempo em segundos que o míssil persegue o alvo antes de seguir direto em linha reta (0 = persegue sempre)")]
    [SerializeField] private float tempoSeguindo = 2.5f;

    [Tooltip("Tag usada para encontrar o alvo automaticamente se não estiver definido")]
    [SerializeField] private string playerTag = "Player";

    private Transform target;
    private float timerHoming = 0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        timerHoming = 0f;
        BuscarAlvo();
    }

    private void BuscarAlvo()
    {
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
            {
                target = player.transform;
            }
        }
    }

    protected override void Move(float multTam, float multTemp)
    {
        timerHoming += Time.fixedDeltaTime;

        // Se o alvo for válido e o tempo de perseguição ainda estiver ativo
        bool podePerseguir = target != null && target.gameObject.activeInHierarchy && (tempoSeguindo <= 0f || timerHoming <= tempoSeguindo);

        if (podePerseguir)
        {
            // Calcula o ângulo em direção ao jogador
            Vector2 direcaoAlvo = ((Vector2)target.position - (Vector2)transform.position).normalized;
            float anguloAlvo = Mathf.Atan2(direcaoAlvo.y, direcaoAlvo.x) * Mathf.Rad2Deg;

            // Interpola a rotação atual até o ângulo do alvo na velocidade definida
            float anguloAtual = transform.eulerAngles.z;
            float novoAngulo = Mathf.MoveTowardsAngle(anguloAtual, anguloAlvo, rotationSpeed * Time.fixedDeltaTime);

            transform.rotation = Quaternion.Euler(0, 0, novoAngulo);
        }

        // Aplica a velocidade na direção em que o míssil está virado (transform.right)
        if (rb != null)
        {
            rb.linearVelocity = (Vector2)transform.right * speed * multTam * multTemp;
        }
    }

    /// <summary>
    /// Permite definir um alvo específico via código (ex: Spawner)
    /// </summary>
    public void SetTarget(Transform novoAlvo)
    {
        target = novoAlvo;
    }
}