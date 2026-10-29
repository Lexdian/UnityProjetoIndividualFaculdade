using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float speed = 2f;

    [Header("Padrões de Movimento")]
    public AbstractEnemyMovement movementPattern;

    [Tooltip("Padrão em Zig-Zag usado automaticamente quando o inimigo for Acelerado")]
    public AbstractEnemyMovement aceleratedMovementPattern;

    private Rigidbody2D rb;
    private EnemyBasics enemyBasics;
    private float timeAlive = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyBasics = GetComponent<EnemyBasics>();
    }

    void OnEnable()
    {
        timeAlive = 0f;
    }

    void FixedUpdate()
    {
        timeAlive += Time.fixedDeltaTime;

        // Se o inimigo for Acelerado e houver o padrão de Zig-Zag configurado, usa ele
        bool isAcelerated = enemyBasics != null && enemyBasics.IsAcelerated;
        bool isGiant = enemyBasics != null && enemyBasics.IsGiant;
        AbstractEnemyMovement activePattern = (isAcelerated && aceleratedMovementPattern != null && !isGiant)
            ? aceleratedMovementPattern
            : movementPattern;

        if (activePattern != null)
        {
            Vector2 dir = activePattern.GetDirection(transform, timeAlive);
            float currentSpeedScale = (enemyBasics != null) ? enemyBasics.speedScale : 1f;

            rb.linearVelocity = dir * speed * currentSpeedScale;
        }

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