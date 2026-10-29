using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening;
using System.Globalization;

public class GameUIManager : MonoBehaviour
{
    private int HighScore = 0;
    private readonly CultureInfo culturaPontos = new CultureInfo("pt-BR");

    [Header("Referências Externas")]
    [SerializeField] private PlayerMovement player;

    [Header("UI do HUD")]
    [SerializeField] private Image caugeEspecial;
    [SerializeField] private Text textoScore;
    [SerializeField] private Text textoHighScore;
    [SerializeField] private Text textoScoreMultiplier;
    [SerializeField] private Text textoVidas;

    [Header("Power-Ups")]
    [SerializeField] private Sprite imagemCimaNula;
    [SerializeField] private Sprite imagemBaixoNula;
    [SerializeField] public int cima = -1;
    [SerializeField] public int baixo = -1;
    [SerializeField] private Image imagemCimaUI;
    [SerializeField] private Image imagemBaixoUI;
    [SerializeField] private Text textoPoder;
    [SerializeField] private Text textoPods;

    [Header("Efeitos de Partículas e Áudio")]
    [SerializeField] private ParticleSystem[] particulasPowerUp;
    [SerializeField] private AudioClip somImpactoPowerUp; // Som do impacto / faísca do Power-Up
    [Range(0f, 1f)][SerializeField] private float volumeImpactoPowerUp = 0.8f;
    private AudioSource audioSource;

    [Header("Configurações de Animação (DOTween)")]
    [SerializeField] private float distanciaAfastamento = 30f;
    [SerializeField] private float duracaoSeparacao = 0.2f;
    [SerializeField] private float duracaoEncontro = 0.15f;
    [SerializeField] private float duracaoBrilho = 0.25f;

    private bool processandoAnimacaoPowerUp = false;
    private Vector3 posicaoOriginalCima;
    private Vector3 posicaoOriginalBaixo;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        HighScore = GameManager.Instance.HighScore;
        if (textoHighScore != null) textoHighScore.text = FormatarPontos(HighScore);

        if (imagemCimaUI != null) posicaoOriginalCima = imagemCimaUI.transform.localPosition;
        if (imagemBaixoUI != null) posicaoOriginalBaixo = imagemBaixoUI.transform.localPosition;
    }

    void Update()
    {
        AtualizarInterface();
    }

    private string FormatarPontos(int valor)
    {
        return valor.ToString("#,##0", culturaPontos);
    }

    private void AtualizarInterface()
    {
        if (player == null) return;

        if (caugeEspecial != null)
        {
            caugeEspecial.fillAmount = player.especialAtual / player.especialMaximo;
            caugeEspecial.color = Color.Lerp(Color.white, Color.red, player.especialAtual / player.especialMaximo);
        }

        // Atualiza a pontuação atual formatada com pontos
        if (textoScore != null)
        {
            textoScore.text = FormatarPontos(player.score);
        }

        // Atualiza o HighScore se a pontuação atual superá-lo
        int highScoreExibicao = Mathf.Max(HighScore, player.score);
        if (textoHighScore != null)
        {
            textoHighScore.text = FormatarPontos(highScoreExibicao);
        }

        if (textoScoreMultiplier != null)
        {
            textoScoreMultiplier.text = "x" + player.scoreMultiplier.ToString();
        }
        if (textoVidas != null)
        {
            textoVidas.text = "x" + player.vidas.ToString();
        }

        AtualizarPowerUps();

        if (textoPoder != null) textoPoder.text = player.tiros + "/5";
        if (textoPods != null) textoPods.text = player.listaModulos.Count + "/3";
    }

    private void AtualizarPowerUps()
    {
        if (processandoAnimacaoPowerUp) return;

        if (cima >= 0 && baixo >= 0)
        {
            // 1. ESPADA (cima = 0, baixo = 0)
            if (cima == 0 && baixo == 0)
            {
                ExecutarAnimacaoCombinacaoPowerUp(() =>
                {
                    if (player.tiros < 5)
                    {
                        player.tiros++;
                    }
                    else
                    {
                        player.GanharPontos(1000);
                    }
                    Debug.Log("Efeito ativado: ESPADA");
                });
            }
            // 2. NAVE (cima = 0, baixo = 1)
            else if (cima == 0 && baixo == 1)
            {
                ExecutarAnimacaoCombinacaoPowerUp(() =>
                {
                    if (player.listaModulos.Count < 3)
                    {
                        player.AdicionarModulo();
                    }
                    else
                    {
                        player.GanharPontos(1000);
                    }
                    Debug.Log("Efeito ativado: NAVE");
                });
            }
            // 3. VIDA (cima = 1, baixo = 2)
            else if (cima == 1 && baixo == 2)
            {
                ExecutarAnimacaoCombinacaoPowerUp(() =>
                {
                    player.vidas++;
                    Debug.Log("Efeito ativado: VIDA +1");
                });
            }
            // 4. ESCUDO (cima = 1, baixo = 3)
            else if (cima == 1 && baixo == 3)
            {
                ExecutarAnimacaoCombinacaoPowerUp(() =>
                {
                    if (player.temEscudo)
                    {
                        player.GanharPontos(1000);
                    }
                    else
                    {
                        player.temEscudo = true;
                    }
                    Debug.Log("Efeito ativado: ESCUDO");
                });
            }
        }
    }

    private void ExecutarAnimacaoCombinacaoPowerUp(System.Action aoConcluirEfeito)
    {
        processandoAnimacaoPowerUp = true;

        imagemCimaUI.transform.DOKill();
        imagemBaixoUI.transform.DOKill();
        imagemCimaUI.DOKill();
        imagemBaixoUI.DOKill();

        Sequence sequenciaPowerUp = DOTween.Sequence();

        // 1. SEPARAR
        sequenciaPowerUp.Append(imagemCimaUI.transform.DOLocalMoveY(posicaoOriginalCima.y + distanciaAfastamento, duracaoSeparacao).SetEase(Ease.OutQuad));
        sequenciaPowerUp.Join(imagemBaixoUI.transform.DOLocalMoveY(posicaoOriginalBaixo.y - distanciaAfastamento, duracaoSeparacao).SetEase(Ease.OutQuad));

        // 2. JUNTA COM IMPACTO
        sequenciaPowerUp.Append(imagemCimaUI.transform.DOLocalMoveY(posicaoOriginalCima.y, duracaoEncontro).SetEase(Ease.InQuad));
        sequenciaPowerUp.Join(imagemBaixoUI.transform.DOLocalMoveY(posicaoOriginalBaixo.y, duracaoEncontro).SetEase(Ease.InQuad));

        // 3. DISPARA AS PARTÍCULAS E O SOM NO MOMENTO DO IMPACTO
        sequenciaPowerUp.AppendCallback(DispararParticulasPowerUp);

        // 4. BRILHO DE IMPACTO
        Color corOriginalCima = imagemCimaUI.color;
        Color corOriginalBaixo = imagemBaixoUI.color;

        sequenciaPowerUp.Append(imagemCimaUI.DOColor(Color.yellow, duracaoBrilho * 0.5f));
        sequenciaPowerUp.Join(imagemBaixoUI.DOColor(Color.yellow, duracaoBrilho * 0.5f));

        sequenciaPowerUp.Append(imagemCimaUI.DOColor(corOriginalCima, duracaoBrilho * 0.5f));
        sequenciaPowerUp.Join(imagemBaixoUI.DOColor(corOriginalBaixo, duracaoBrilho * 0.5f));

        // 5. CONCLUSÃO DA ANIMAÇÃO
        sequenciaPowerUp.OnComplete(() =>
        {
            imagemCimaUI.transform.localPosition = posicaoOriginalCima;
            imagemBaixoUI.transform.localPosition = posicaoOriginalBaixo;

            imagemCimaUI.transform.localScale = Vector3.one;
            imagemBaixoUI.transform.localScale = Vector3.one;

            imagemCimaUI.sprite = imagemCimaNula;
            imagemBaixoUI.sprite = imagemBaixoNula;

            cima = -1;
            baixo = -1;

            PartePoder.AtualizarTodasPartesNaTela();

            aoConcluirEfeito?.Invoke();

            processandoAnimacaoPowerUp = false;
        });
    }

    private void DispararParticulasPowerUp()
    {
        // Toca o som no instante exato do choque/faíscas
        if (somImpactoPowerUp != null && audioSource != null)
        {
            audioSource.PlayOneShot(somImpactoPowerUp, volumeImpactoPowerUp);
        }

        if (particulasPowerUp == null) return;

        foreach (var ps in particulasPowerUp)
        {
            if (ps != null)
            {
                ps.Play();
            }
        }
    }
}