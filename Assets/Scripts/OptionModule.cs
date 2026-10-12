using UnityEngine;

public class OptionModule : MonoBehaviour
{
    [Header("Configurações do Módulo")]
    [SerializeField] private int delayFrames = 10;
    [SerializeField] private Vector2 offsetFoco = new Vector2(0.5f, 0f);
    [SerializeField] private float velocidadeFoco = 12f;
    [SerializeField] private float velocidadeRetornoRastro = 10f; // <--- Suaviza a volta para o rastro!

    private PlayerMovement player;
    private BulletPool pool;
    private int indiceModulo = 0;

    public void Inicializar(PlayerMovement playerRef, BulletPool poolRef, int indice)
    {
        player = playerRef;
        pool = poolRef;
        indiceModulo = indice;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // SE ESTIVER EM FOCO: Junta próximo ao jogador
        if (player.EmFoco)
        {
            float direcaoX = (indiceModulo % 2 == 0) ? 1f : -1f;
            int camada = (indiceModulo / 2) + 1;

            Vector3 posicaoAlvo = player.transform.position + new Vector3(
                offsetFoco.x,
                offsetFoco.y,
                0f
            );

            transform.position = Vector3.Lerp(transform.position, posicaoAlvo, Time.deltaTime * velocidadeFoco);
        }
        // SE NÃO ESTIVER EM FOCO: Transiciona suavemente de volta para o rastro
        else
        {
            int frameAtrasoDesejado = delayFrames * (indiceModulo + 1);
            Vector3 posicaoHistorica = player.ObterPosicaoHistorico(frameAtrasoDesejado);

            // <--- LERP em vez de atribuição direta para eliminar o teleport!
            transform.position = Vector3.Lerp(transform.position, posicaoHistorica, Time.deltaTime * velocidadeRetornoRastro);

            // Atualiza o offset relativo para manter a referência
            offsetFoco = this.transform.position - player.transform.position;
        }
    }

    public void Disparar()
    {
        if (pool == null) return;

        GameObject bullet = pool.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = transform.position;
            BulletMovement bm = bullet.GetComponent<BulletMovement>();
            if (bm != null)
            {
                bm.SetPool(pool);
                bm.SetDirection(Vector2.up);
            }
        }
    }
}