using System.Collections.Generic;
using UnityEngine;

public static class ExplosiveForceManager
{
    /// <summary>
    /// Aplica uma força impulsiva radial em uma lista de objetos a partir de um ponto de origem.
    /// </summary>
    /// <param name="objetos">Lista de GameObjects para impulsionar</param>
    /// <param name="pontoOrigem">Posição central da explosão</param>
    /// <param name="forcaMinima">Força mínima do impulso</param>
    /// <param name="forcaMaxima">Força máxima do impulso</param>
    /// <param name="torqueMaximo">Rotação aleatória aplicada ao objeto</param>
    public static void AplicarExplosao2D(List<GameObject> objetos, Vector3 pontoOrigem, float forcaMinima = 3f, float forcaMaxima = 7f, float torqueMaximo = 40f)
    {
        foreach (GameObject obj in objetos)
        {
            if (obj == null || !obj.activeInHierarchy) continue;

            // Pega o Rigidbody2D (ou adiciona um se não existir)
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 0f; // Ajuste conforme a física do seu jogo
                rb.freezeRotation = true;
            }

            // Calcula a direção a partir da origem
            Vector2 direcao = (obj.transform.position - pontoOrigem).normalized;

            // Se o objeto estiver exatamente na mesma posição da origem, sorteia um ângulo aleatório
            if (direcao == Vector2.zero)
            {
                direcao = Random.insideUnitCircle.normalized;
            }

            // Aplica a força de impulso aleatória dentro do intervalo
            float forca = Random.Range(forcaMinima, forcaMaxima);
            rb.AddForce(direcao * forca, ForceMode2D.Impulse);

            // Adiciona rotação/torque para o efeito ficar mais dinâmico
            if (torqueMaximo > 0f)
            {
                float torque = Random.Range(-torqueMaximo, torqueMaximo);
                rb.AddTorque(torque, ForceMode2D.Impulse);
            }
        }
    }
}