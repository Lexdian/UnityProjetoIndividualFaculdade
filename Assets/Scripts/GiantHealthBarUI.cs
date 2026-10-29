using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GiantHealthBarUI : MonoBehaviour
{
    public static GiantHealthBarUI Instance { get; private set; }

    [Header("Componentes de UI")]
    [SerializeField] private CanvasGroup canvasGroupGeral; // Controla visibilidade do conjunto todo
    [SerializeField] private CanvasGroup molduraCanvasGroup; // CanvasGroup exclusivo da Moldura para piscar
    [SerializeField] private RectTransform fillBarraTransform; // RectTransform da BARRA DE VIDA vermelha

    [Header("UI do Alerta de Ameaça (Centro da Tela)")]
    [SerializeField] private CanvasGroup alertCanvasGroup; // CanvasGroup do painel de alerta
    [SerializeField] private RectTransform alertContainer; // RectTransform para animação de escala/impacto
    [SerializeField] private TMP_Text alertText; // Texto de Alerta (ex: "colossus detected!")
    [SerializeField] private float duracaoExibicaoAlerta = 1.0f;

    [Header("Áudio do Alerta")]
    [SerializeField] private AudioSource audioSourceUI; // Componente de áudio (se nulo, busca automático)
    [SerializeField] private AudioClip somAlarme; // Clip de áudio do alarme/sirene
    [Range(0f, 1f)][SerializeField] private float volumeAlarme = 0.8f;
    [SerializeField] private float duracaoSomAlarme = 0.8f; // Duração máxima para interromper o som do alarme

    [Header("Configurações do Pisca da Moldura")]
    [SerializeField] private int quantidadePiscas = 3;
    [SerializeField] private float duracaoCadaPisca = 0.08f;

    [Header("Configurações de Preenchimento da Vida")]
    [SerializeField] private float duracaoPreenchimentoInicial = 1.2f;
    [SerializeField] private Ease curvaPreenchimentoInicial = Ease.OutQuad;

    private EnemyBasics giganteAtual;
    private float MaxVida;
    private Tween tweenSequencia;
    private float percentualVidaExibida = 0f;
    private bool animacaoEntradaConcluida = false;
    private bool estaFechando = false; // Trava para evitar chamadas duplicadas no Update

    private Vector3 escalaOriginalAlerta = Vector3.one;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (canvasGroupGeral == null) canvasGroupGeral = GetComponent<CanvasGroup>();

        if (alertContainer != null)
        {
            escalaOriginalAlerta = alertContainer.localScale;
        }

        if (audioSourceUI == null)
        {
            audioSourceUI = GetComponent<AudioSource>();
            if (audioSourceUI == null)
            {
                audioSourceUI = gameObject.AddComponent<AudioSource>();
            }
        }
        audioSourceUI.playOnAwake = false;

        ResetBarra();
    }

    private void ResetBarra()
    {
        estaFechando = false;
        if (canvasGroupGeral != null) canvasGroupGeral.alpha = 0f;
        if (molduraCanvasGroup != null) molduraCanvasGroup.alpha = 1f;

        if (alertCanvasGroup != null) alertCanvasGroup.alpha = 0f;
        if (alertContainer != null) alertContainer.localScale = Vector3.zero;

        if (fillBarraTransform != null)
        {
            fillBarraTransform.pivot = new Vector2(0.5f, fillBarraTransform.pivot.y);
            fillBarraTransform.localScale = new Vector3(0f, 1f, 1f);
        }
        percentualVidaExibida = 0f;
        animacaoEntradaConcluida = false;

        if (audioSourceUI != null) audioSourceUI.Stop();
    }

    public void AbrirBarraVida(EnemyBasics gigante, string nomeExibicao = "heavy colossus")
    {
        if (gigante == null) return;

        estaFechando = false;
        giganteAtual = gigante;
        MaxVida = gigante.currentHealth;

        tweenSequencia?.Kill();

        percentualVidaExibida = 0f;
        animacaoEntradaConcluida = false;

        if (fillBarraTransform != null)
        {
            fillBarraTransform.pivot = new Vector2(0.5f, fillBarraTransform.pivot.y);
            fillBarraTransform.localScale = new Vector3(0f, 1f, 1f);
        }

        if (alertCanvasGroup != null) alertCanvasGroup.alpha = 0f;
        if (alertContainer != null) alertContainer.localScale = Vector3.zero;

        if (alertText != null)
        {
            alertText.text = ObterTextoAmeaca(gigante);
        }

        canvasGroupGeral.alpha = 1f;

        TocarAlarme();

        Sequence seq = DOTween.Sequence();

        if (alertCanvasGroup != null && alertContainer != null)
        {
            seq.Append(alertCanvasGroup.DOFade(1f, 0.15f));
            seq.Join(alertContainer.DOScale(escalaOriginalAlerta, 0.2f).SetEase(Ease.OutBack));
            seq.Append(alertContainer.DOPunchScale(escalaOriginalAlerta * 0.15f, 0.35f, 8, 0.5f));
            seq.AppendInterval(duracaoExibicaoAlerta);

            seq.Append(alertCanvasGroup.DOFade(0f, 0.2f));
            seq.Join(alertContainer.DOScale(Vector3.zero, 0.2f).SetEase(Ease.InBack));
        }

        if (molduraCanvasGroup != null)
        {
            for (int i = 0; i < quantidadePiscas; i++)
            {
                seq.Append(molduraCanvasGroup.DOFade(0.1f, duracaoCadaPisca));
                seq.Append(molduraCanvasGroup.DOFade(1f, duracaoCadaPisca));
            }
        }

        seq.Append(DOTween.To(() => percentualVidaExibida, x => {
            percentualVidaExibida = x;
            if (fillBarraTransform != null)
            {
                fillBarraTransform.localScale = new Vector3(percentualVidaExibida, 1f, 1f);
            }
        }, 1f, duracaoPreenchimentoInicial).SetEase(curvaPreenchimentoInicial));

        seq.OnComplete(() =>
        {
            animacaoEntradaConcluida = true;
        });

        tweenSequencia = seq;
    }

    private void TocarAlarme()
    {
        if (somAlarme == null || audioSourceUI == null) return;

        audioSourceUI.DOKill();
        audioSourceUI.Stop();

        audioSourceUI.clip = somAlarme;
        audioSourceUI.volume = volumeAlarme;
        audioSourceUI.Play();

        DOVirtual.DelayedCall(duracaoSomAlarme, () =>
        {
            if (audioSourceUI != null && audioSourceUI.isPlaying)
            {
                audioSourceUI.Stop();
            }
        }).SetUpdate(true);
    }

    private string ObterTextoAmeaca(EnemyBasics eb)
    {
        if (eb.IsAcelerated && eb.IsMutant) return "warning: triple threat colossus!";
        if (eb.IsAcelerated) return "warning: accelerated colossus!";
        if (eb.IsMutant) return "warning: mutant colossus!";
        return "warning: colossus detected!";
    }

    private void Update()
    {
        if (giganteAtual == null || !giganteAtual.gameObject.activeInHierarchy)
        {
            // Dispara o fechamento apenas uma vez quando a barra ainda não começou a fechar
            if (!estaFechando && canvasGroupGeral != null && canvasGroupGeral.alpha > 0f)
            {
                FecharBarraVida();
            }
            return;
        }

        if (animacaoEntradaConcluida)
        {
            float percentualReal = Mathf.Clamp01(giganteAtual.currentHealth / MaxVida);

            percentualVidaExibida = Mathf.Lerp(percentualVidaExibida, percentualReal, Time.deltaTime * 10f);

            if (fillBarraTransform != null)
            {
                fillBarraTransform.localScale = new Vector3(percentualVidaExibida, 1f, 1f);
            }
        }
    }

    public void FecharBarraVida()
    {
        if (estaFechando) return;
        estaFechando = true;

        giganteAtual = null;
        animacaoEntradaConcluida = false;
        tweenSequencia?.Kill();

        if (audioSourceUI != null)
        {
            audioSourceUI.DOKill();
            audioSourceUI.Stop();
        }

        Sequence seq = DOTween.Sequence();
        if (fillBarraTransform != null)
        {
            seq.Append(fillBarraTransform.DOScaleX(0f, 0.2f).SetEase(Ease.InQuad));
        }
        if (canvasGroupGeral != null)
        {
            seq.Join(canvasGroupGeral.DOFade(0f, 0.3f));
        }

        tweenSequencia = seq;
    }
}