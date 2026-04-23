using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class GrazeVisual : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private CircleCollider2D grazeCollider;

    [Header("Configurações Visuais")]
    public int segmentos = 36;
    public float duracaoEfeito = 0.2f;
    public Color corGraze = Color.cyan;

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        grazeCollider = GetComponent<CircleCollider2D>();

        lineRenderer.positionCount = segmentos;
        lineRenderer.useWorldSpace = false;
        lineRenderer.enabled = false;

        lineRenderer.startWidth = 0.02f;
        lineRenderer.endWidth = 0.02f;

        DesenharCirculo();
    }

    void DesenharCirculo()
    {
        float raio = grazeCollider.radius;

        float angulo = 0f;

        for (int i = 0; i < segmentos; i++)
        {
            float rad = Mathf.Deg2Rad * angulo;

            float x = Mathf.Sin(rad) * raio;
            float y = Mathf.Cos(rad) * raio;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));

            angulo += 360f / segmentos;
        }
    }

    public void MostrarFeedback()
    {
        StopAllCoroutines();
        StartCoroutine(EfeitoFlash());
    }

    IEnumerator EfeitoFlash()
    {
        lineRenderer.enabled = true;
        lineRenderer.startColor = corGraze;
        lineRenderer.endColor = corGraze;

        yield return new WaitForSeconds(duracaoEfeito);

        lineRenderer.enabled = false;
    }
}