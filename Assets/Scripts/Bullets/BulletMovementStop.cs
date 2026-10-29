using UnityEngine;

public class BulletMovementStop : BulletMovement
{
    [Header("Configurações do Padrão Parar/Andar")]
    [Tooltip("Tempo padrão em segundos que a bala anda nos ciclos normais")]
    [SerializeField] private float tempoAndando = 0.8f;

    [Tooltip("Tempo em segundos que a bala fica completamente parada")]
    [SerializeField] private float tempoParado = 0.4f;

    private float tempoPrimeiraCaminhada = -1f;
    private bool jaFezPrimeiraParada = false;

    private float timerEstado = 0f;
    private bool estaParado = false;
    private Rigidbody2D rbChild;

    // Usamos 'protected override' e chamamos 'base.Awake()' para garantir que a escala inicial seja salva!
    protected override void Awake()
    {
        base.Awake(); // Salva a escala inicial (0.25) e garante o Rigidbody
        rbChild = rb; // Reaproveita a referência do Rigidbody da classe base
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        timerEstado = 0f;
        estaParado = false;
        jaFezPrimeiraParada = false;
        tempoPrimeiraCaminhada = -1f;
    }

    public void ConfigurarEscalonamento(float tempoPrimeiroPasso, float tempoAndandoPadrao, float tempoParadoPadrao)
    {
        tempoPrimeiraCaminhada = tempoPrimeiroPasso;
        tempoAndando = tempoAndandoPadrao;
        tempoParado = tempoParadoPadrao;
    }

    protected override void Move(float multTam, float multTemp)
    {
        timerEstado += Time.fixedDeltaTime;

        float tempoCaminhadaAtual = (!jaFezPrimeiraParada && tempoPrimeiraCaminhada > 0f)
            ? tempoPrimeiraCaminhada
            : tempoAndando;

        if (estaParado)
        {
            if (rbChild != null)
            {
                rbChild.linearVelocity = Vector2.zero;
            }

            if (timerEstado >= tempoParado)
            {
                estaParado = false;
                timerEstado = 0f;
                jaFezPrimeiraParada = true;
            }
        }
        else
        {
            base.Move(multTam, multTemp);

            if (timerEstado >= tempoCaminhadaAtual)
            {
                estaParado = true;
                timerEstado = 0f;
            }
        }
    }
}