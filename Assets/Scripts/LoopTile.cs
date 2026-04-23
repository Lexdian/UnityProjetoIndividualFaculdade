using UnityEngine;

public class LoopTile : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    public float velocidade = 5f;

    [Tooltip("Quando o Y chegar neste valor negativo, ele reseta.")]
    public float limiteSairDaTela = -10f;

    [Tooltip("Para onde o objeto volta (geralmente 0 ou o topo da tela).")]
    public float posicaoReset = 10f;

    void Update()
    {
        // Move o objeto para baixo constantemente
        transform.Translate(Vector2.down * velocidade * Time.deltaTime);

        // Verifica se ele passou do limite
        if (transform.position.y <= limiteSairDaTela)
        {
            // Reseta a posição mantendo o X e o Z originais
            transform.position = new Vector3(transform.position.x, posicaoReset, transform.position.z);
        }
    }
}