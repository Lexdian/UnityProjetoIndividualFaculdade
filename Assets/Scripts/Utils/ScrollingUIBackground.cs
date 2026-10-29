using UnityEngine;
using UnityEngine.UI;

public class ScrollingUIBackground : MonoBehaviour
{
    [Header("Velocidade do Movimento UV")]
    [SerializeField] private Vector2 velocidadeScroll = new Vector2(0.5f, 0f);

    private RawImage rawImage;

    private void Start()
    {
        rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (rawImage != null)
        {
            Rect uvRectAtual = rawImage.uvRect;
            uvRectAtual.x += velocidadeScroll.x * Time.deltaTime;
            uvRectAtual.y += velocidadeScroll.y * Time.deltaTime;
            rawImage.uvRect = uvRectAtual;
        }
    }
}