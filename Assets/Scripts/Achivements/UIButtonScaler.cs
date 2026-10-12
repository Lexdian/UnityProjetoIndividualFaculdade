using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonScaler : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    [Header("Configurações de Escala")]
    [SerializeField] private Vector3 normalScale = Vector3.one;
    [SerializeField] private Vector3 selectedScale = new Vector3(1.15f, 1.15f, 1.15f);
    [SerializeField] private Vector3 pressedScale = new Vector3(1.05f, 1.05f, 1.05f);
    [SerializeField] private float duration = 0.15f;

    private Coroutine scaleCoroutine;
    private bool isSelected = false;

    private void OnDisable()
    {
        transform.localScale = normalScale;
        isSelected = false;
    }

    // Quando o mouse passa por cima
    public void OnPointerEnter(PointerEventData eventData)
    {
        StartScaleAnimation(selectedScale);
    }

    // Quando o mouse sai de cima
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            StartScaleAnimation(normalScale);
    }

    // Quando o botão é selecionado (Teclado/Gamepad/Navegação UI)
    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
        StartScaleAnimation(selectedScale);
    }

    // Quando perde a seleção
    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        StartScaleAnimation(normalScale);
    }

    // Quando o botão é pressionado/clicado
    public void OnPointerDown(PointerEventData eventData)
    {
        StartScaleAnimation(pressedScale);
    }

    // Quando o clique/pressionamento é solto
    public void OnPointerUp(PointerEventData eventData)
    {
        StartScaleAnimation(selectedScale);
    }

    private void StartScaleAnimation(Vector3 targetScale)
    {
        if (!gameObject.activeInHierarchy) return;

        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(AnimateScale(targetScale));
    }

    private IEnumerator AnimateScale(Vector3 target)
    {
        Vector3 startScale = transform.localScale;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsedTime / duration);
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localScale = Vector3.Lerp(startScale, target, t);
            yield return null;
        }

        transform.localScale = target;
    }
}