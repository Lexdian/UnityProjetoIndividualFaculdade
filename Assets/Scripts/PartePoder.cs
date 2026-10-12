using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PartePoder : Collectible
{
    [Header("Identificação da Parte")]
    [Tooltip("Marque verdadeiro se for a parte de CIMA, desmarque se for a parte de BAIXO.")]
    [SerializeField] private bool isParteDeCima = true;

    [Header("Configurações da UI")]
    [SerializeField] private RectTransform uiCanvasParent;       // O Canvas principal
    [SerializeField] private RectTransform targetContainerCima;  // Container permanente da parte de CIMA
    [SerializeField] private RectTransform targetContainerBaixo; // Container permanente da parte de BAIXO
    [SerializeField] private GameObject uiPrefabToAnimate;        // Prefab temporário apenas para a animação

    [Header("Configurações da Animação")]
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float peakScale = 1.4f;

    public int Tipo;
    public Sprite[] imagesCima;
    public Sprite[] imagesBaixo;
    public GameUIManager gameUIManager;

    // Guarda o sprite escolhido no Start/OnEnable de forma definitiva
    private Sprite spriteAtribuido;

    protected override void Start()
    {
        base.Start(); // Executa o Start da classe base (busca a Camera.main)

        if (uiCanvasParent == null)
            uiCanvasParent = GameObject.FindWithTag("CanvasParent")?.GetComponent<RectTransform>();

        if (targetContainerCima == null)
            targetContainerCima = GameObject.FindWithTag("ContainerCima")?.GetComponent<RectTransform>();

        if (targetContainerBaixo == null)
            targetContainerBaixo = GameObject.FindWithTag("ContainerBaixo")?.GetComponent<RectTransform>();

        if (gameUIManager == null)
            gameUIManager = GameObject.FindWithTag("GameUIManager")?.GetComponent<GameUIManager>();

        // Inicializa Tipo e spriteAtribuido (também será chamado em OnEnable para suportar reativações)
        AssignSprite();

        scoreValue = 0; // Define o valor do score como 0, já que não é relevante para a coleta
    }

    void OnEnable()
    {
        // Recalcula/garante sprite sempre que o objeto for ativado (evita problema com pooling/reuso)
        AssignSprite();
    }

    private void AssignSprite()
    {
        spriteAtribuido = null;

        if (gameUIManager != null && gameUIManager.cima < 0)
        {
            // Parte de cima
            isParteDeCima = true;
            if (imagesCima != null && imagesCima.Length > 0)
            {
                Tipo = Random.Range(0, imagesCima.Length);
                int spriteIndex = Mathf.Clamp(Tipo, 0, imagesCima.Length - 1);
                spriteAtribuido = imagesCima[spriteIndex];
            }
            else
            {
                Tipo = 0;
            }
        }
        else
        {
            // Parte de baixo
            isParteDeCima = false;
            int inicio = (gameUIManager != null && gameUIManager.cima == 0) ? 0 : 2;
            int fim = (gameUIManager != null && gameUIManager.cima == 0) ? 1 : 3;

            Tipo = Random.Range(inicio, fim + 1);

            if (imagesBaixo != null && imagesBaixo.Length > 0)
            {
                int relativeIndex = Tipo - inicio;

                if (relativeIndex >= 0 && relativeIndex < imagesBaixo.Length)
                {
                    spriteAtribuido = imagesBaixo[relativeIndex];
                }
                else if (Tipo >= 0 && Tipo < imagesBaixo.Length)
                {
                    // fallback: usa Tipo direto se couber
                    spriteAtribuido = imagesBaixo[Tipo];
                }
                else
                {
                    // fallback seguro: último recurso usa índice 0
                    spriteAtribuido = imagesBaixo[0];
                }
            }
            else
            {
                // se não há sprites em imagesBaixo, garante um Tipo válido
                Tipo = Mathf.Clamp(Tipo, 0, 0);
            }
        }

        // Aplica o sprite no SpriteRenderer (se houver)
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr))
        {
            if (spriteAtribuido != null)
                sr.sprite = spriteAtribuido;
            // se spriteAtribuido for nulo, deixamos o sprite atual como está (evita sobrescrever com null)
        }

#if UNITY_EDITOR
        // Log útil para depuração durante testes; remova ou comente em build
        // Debug.Log($"PartePoder.AssignSprite: isParteDeCima={isParteDeCima} Tipo={Tipo} spriteAssigned={(spriteAtribuido!=null)}");
#endif
    }

    public override void Coletar()
    {
        // Desativa a colisão e a imagem do mundo imediatamente
        if (TryGetComponent<Collider2D>(out Collider2D col)) col.enabled = false;
        if (TryGetComponent<SpriteRenderer>(out SpriteRenderer sr)) sr.enabled = false;

        AnimateToUI();
    }

    private void AnimateToUI()
    {
        RectTransform targetUIElement = isParteDeCima ? targetContainerCima : targetContainerBaixo;

        if (targetUIElement == null || uiCanvasParent == null || uiPrefabToAnimate == null)
        {
            Debug.LogError("Referências de UI ausentes no Inspector de PartePoder!");
            Destroy(gameObject);
            return;
        }

        Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position);

        GameObject uiIcon = Instantiate(uiPrefabToAnimate, uiCanvasParent);

        if (uiIcon.TryGetComponent<Image>(out Image iconImage))
        {
            // Aplicar sprite apenas se definido; evita setar null
            if (spriteAtribuido != null)
                iconImage.sprite = spriteAtribuido;
        }

        RectTransform iconRect = uiIcon.GetComponent<RectTransform>();
        iconRect.position = screenPos;
        iconRect.rotation = transform.rotation;

        Sequence collectSeq = DOTween.Sequence();

        collectSeq.Append(iconRect.DOScale(Vector3.one * peakScale, duration * 0.3f).SetEase(Ease.OutBack));
        collectSeq.Append(iconRect.DOScale(Vector3.one * 0.5f, duration * 0.7f).SetEase(Ease.InBack));
        collectSeq.Insert(0, iconRect.DOMove(targetUIElement.position, duration).SetEase(Ease.InOutQuad));

        collectSeq.OnComplete(() =>
        {
            if (targetUIElement.TryGetComponent<Image>(out Image targetImage))
            {
                if (spriteAtribuido != null)
                {
                    targetImage.sprite = spriteAtribuido;

                    Color fullAlpha = targetImage.color;
                    fullAlpha.a = 1f;
                    targetImage.color = fullAlpha;
                }
            }

            targetUIElement.DOPunchScale(Vector3.one * 0.25f, 0.2f);

            Destroy(uiIcon);

            if (gameUIManager != null)
            {
                if (isParteDeCima)
                    gameUIManager.cima = Tipo;
                else
                    gameUIManager.baixo = Tipo;

                targetUIElement.DOScale(Vector3.one, 0.2f);
            }

            Destroy(gameObject);
        });
    }
}