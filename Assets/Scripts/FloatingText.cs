using System.Collections;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private TMP_Text textMesh;

    [Header("Configurações de Animação")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private Vector3 moveDirection = Vector3.up;

    private void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TMP_Text>();
    }

    public void Setup(string message, Color color, float fontSize = 0f)
    {
        if (textMesh == null) textMesh = GetComponent<TMP_Text>();

        textMesh.text = message;
        textMesh.color = color;

        if (fontSize > 0)
        {
            textMesh.fontSize = fontSize;
        }

        StopAllCoroutines();
        StartCoroutine(AnimateAndDisable());
    }

    private IEnumerator AnimateAndDisable()
    {
        float elapsedTime = 0f;
        Color startColor = textMesh.color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            // Move o texto na direção configurada
            transform.position += moveDirection * moveSpeed * Time.deltaTime;

            // Desvanece a transparência (Alpha)
            Color currentColor = startColor;
            currentColor.a = Mathf.Lerp(startColor.a, 0f, t);
            textMesh.color = currentColor;

            yield return null;
        }

        // Desativa para reuso na Pool ou destrói
        gameObject.SetActive(false);
    }
}