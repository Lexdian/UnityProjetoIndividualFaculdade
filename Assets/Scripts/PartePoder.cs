using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PartePoder : Collectible
{
    private static List<PartePoder> partesNaTela = new List<PartePoder>();

    [Header("Identificação da Parte")]
    [Tooltip("Marque verdadeiro se for a parte de CIMA, desmarque se for a parte de BAIXO.")]
    [SerializeField] private bool isParteDeCima = true;

    [Header("Configurações da UI Alvo")]
    [SerializeField] private RectTransform targetContainerCima;
    [SerializeField] private RectTransform targetContainerBaixo;

    [Header("Configurações da Animação")]
    [SerializeField] private float duration = 0.8f;
    [SerializeField] private float peakScale = 1.4f;

    public int Tipo;
    public Sprite[] imagesCima;
    public Sprite[] imagesBaixo;
    public GameUIManager gameUIManager;

    private Sprite spriteAtribuido;
    private Collider2D col2D;
    private SpriteRenderer spriteRenderer;
    private int originalLayer;

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalLayer = gameObject.layer;
        GarantirReferencias();
    }

    protected override void Start()
    {
        base.Start();
        GarantirReferencias();
        scoreValue = 0;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GarantirReferencias();

        // Restaura a camada original e os componentes
        gameObject.layer = originalLayer;
        if (col2D != null) col2D.enabled = true;
        if (spriteRenderer != null) spriteRenderer.enabled = true;

        if (!partesNaTela.Contains(this))
        {
            partesNaTela.Add(this);
        }

        AssignSprite();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        transform.DOKill();

        if (partesNaTela.Contains(this))
        {
            partesNaTela.Remove(this);
        }
    }

    private void GarantirReferencias()
    {
        if (mainCam == null) mainCam = Camera.main;

        if (targetContainerCima == null)
            targetContainerCima = GameObject.FindWithTag("ContainerCima")?.GetComponent<RectTransform>();

        if (targetContainerBaixo == null)
            targetContainerBaixo = GameObject.FindWithTag("ContainerBaixo")?.GetComponent<RectTransform>();

        if (gameUIManager == null)
            gameUIManager = GameObject.FindWithTag("GameUIManager")?.GetComponent<GameUIManager>();
    }

    public void AssignSprite(bool? forcarParteDeCima = null, int? tipoCimaReferencia = null)
    {
        spriteAtribuido = null;

        if (forcarParteDeCima.HasValue)
        {
            isParteDeCima = forcarParteDeCima.Value;
        }
        else
        {
            isParteDeCima = (gameUIManager != null && gameUIManager.cima < 0);
        }

        if (isParteDeCima)
        {
            if (imagesCima != null && imagesCima.Length > 0)
            {
                Tipo = Random.Range(0, imagesCima.Length);
                int spriteIndex = Mathf.Clamp(Tipo, 0, imagesCima.Length - 1);
                spriteAtribuido = imagesCima[spriteIndex];
            }
        }
        else
        {
            int valorCimaRef = 0;
            if (tipoCimaReferencia.HasValue)
            {
                valorCimaRef = tipoCimaReferencia.Value;
            }
            else if (gameUIManager != null && gameUIManager.cima >= 0)
            {
                valorCimaRef = gameUIManager.cima;
            }

            int inicio = (valorCimaRef == 0) ? 0 : 2;
            int fim = (valorCimaRef == 0) ? 1 : 3;

            Tipo = Random.Range(inicio, fim + 1);

            if (imagesBaixo != null && Tipo >= 0 && Tipo < imagesBaixo.Length)
            {
                spriteAtribuido = imagesBaixo[Tipo];
            }
        }

        if (spriteRenderer != null && spriteAtribuido != null)
        {
            spriteRenderer.sprite = spriteAtribuido;
        }
    }

    public override void Coletar()
    {
        if (col2D != null) col2D.enabled = false;

        partesNaTela.Remove(this);

        bool novoGrupoOutros = !isParteDeCima;
        int? refTipoCima = isParteDeCima ? Tipo : (int?)null;

        AtualizarTodasPartesNaTela(novoGrupoOutros, refTipoCima);

        AnimateToUI();
    }

    public static void AtualizarTodasPartesNaTela(bool? forcarParteDeCima = null, int? tipoCimaReferencia = null)
    {
        for (int i = partesNaTela.Count - 1; i >= 0; i--)
        {
            if (partesNaTela[i] != null && partesNaTela[i].gameObject.activeInHierarchy)
            {
                partesNaTela[i].AssignSprite(forcarParteDeCima, tipoCimaReferencia);

                // Cancele animações anteriores no transform antes de aplicar o punch
                partesNaTela[i].transform.DOKill();
                partesNaTela[i].transform.DOPunchScale(Vector3.one * 0.25f, 0.2f);
            }
        }
    }

    private void AnimateToUI()
    {
        RectTransform targetUIElement = isParteDeCima ? targetContainerCima : targetContainerBaixo;

        if (targetUIElement == null)
        {
            FinalizarObjeto();
            return;
        }

        // 1. Identifica o Canvas e a Câmera responsável pela UI
        Canvas parentCanvas = targetUIElement.GetComponentInParent<Canvas>();
        Camera uiCamera = (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            ? parentCanvas.worldCamera
            : null;

        Camera camParaTela = (uiCamera != null) ? uiCamera : mainCam;

        // 2. Troca temporariamente a Layer do objeto para "UI" para ser desenhado pela Câmera de UI em tela cheia
        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer != -1) gameObject.layer = uiLayer;

        // 3. Obtém a posição da UI na Tela (Screen Space)
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, targetUIElement.position);

        // 4. Converte a Posição da Tela para Posição no Mundo da Câmera de UI
        Vector3 targetWorldPos = camParaTela.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(camParaTela.transform.position.z)));
        targetWorldPos.z = 0f;

        // 5. Animação com DOTween
        Sequence collectSeq = DOTween.Sequence().SetTarget(gameObject);

        collectSeq.Append(transform.DOScale(Vector3.one * peakScale, duration * 0.3f).SetEase(Ease.OutBack));
        collectSeq.Append(transform.DOScale(Vector3.one * 0.5f, duration * 0.7f).SetEase(Ease.InBack));
        collectSeq.Insert(0, transform.DOMove(targetWorldPos, duration).SetEase(Ease.InOutQuad));

        collectSeq.OnComplete(() =>
        {
            if (targetUIElement == null) return;

            if (targetUIElement.TryGetComponent<Image>(out Image targetImage) && spriteAtribuido != null)
            {
                targetImage.sprite = spriteAtribuido;

                Color fullAlpha = targetImage.color;
                fullAlpha.a = 1f;
                targetImage.color = fullAlpha;
            }

            // Interrompe qualquer tween de escala anterior no container para evitar conflitos
            targetUIElement.DOKill();
            targetUIElement.localScale = Vector3.one;

            // Executa o pulso no container de UI e garante o reset da escala ao concluir
            targetUIElement.DOPunchScale(Vector3.one * 0.25f, 0.2f).OnComplete(() =>
            {
                if (targetUIElement != null) targetUIElement.localScale = Vector3.one;
            });

            if (gameUIManager != null)
            {
                if (isParteDeCima)
                    gameUIManager.cima = Tipo;
                else
                    gameUIManager.baixo = Tipo;
            }

            FinalizarObjeto();
        });
    }

    private void FinalizarObjeto()
    {
        gameObject.SetActive(false);
    }
}