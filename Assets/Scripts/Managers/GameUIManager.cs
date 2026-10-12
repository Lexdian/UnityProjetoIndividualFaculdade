using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using DG.Tweening; // <--- Importante adicionar para usar o DOTween!

public class GameUIManager : MonoBehaviour
{
    private int HighScore = 0;

    [Header("Referências Externas")]
    [SerializeField] private PlayerMovement player;

    [Header("UI do HUD")]
    [SerializeField] private Image caugeEspecial;
    [SerializeField] private Text textoScore;
    [SerializeField] private Text textoHighScore;
    [SerializeField] private Text textoScoreMultiplier;
    [SerializeField] private Text textoVidas;

    [Header("Configurações de Pause")]
    [SerializeField] private GameObject painelPause;
    private bool jogoPausado = false;

    [Header("Power-Ups")]
    [SerializeField] private Sprite imagemCimaNula;
    [SerializeField] private Sprite imagemBaixoNula;
    [SerializeField] public int cima = -1;
    [SerializeField] public int baixo = -1;
    [SerializeField] private Image imagemCimaUI;
    [SerializeField] private Image imagemBaixoUI;
    [SerializeField] private Text textoPoder;
    [SerializeField] private Text textoPods;

    [Header("Configurações de Animação (DOTween)")]
    [SerializeField] private float distanciaAfastamento = 30f; // Quanto as imagens se afastam (em pixels/unidades)
    [SerializeField] private float duracaoSeparacao = 0.2f;
    [SerializeField] private float duracaoEncontro = 0.15f;
    [SerializeField] private float duracaoBrilho = 0.25f;

    private bool processandoAnimacaoPowerUp = false; // Evita disparar a animação múltiplas vezes em loop
    private Vector3 posicaoOriginalCima;
    private Vector3 posicaoOriginalBaixo;

    private void Start()
    {
        HighScore = GameManager.Instance.HighScore;
        if (textoHighScore != null) textoHighScore.text = HighScore.ToString("F0");

        // Salva as posições originais das imagens de UI para resetar após as animações
        if (imagemCimaUI != null) posicaoOriginalCima = imagemCimaUI.transform.localPosition;
        if (imagemBaixoUI != null) posicaoOriginalBaixo = imagemBaixoUI.transform.localPosition;
    }

    void Update()
    {
        // 1. Controle de Pause
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            if (jogoPausado)
                RetomarJogo();
            else
                PausarJogo();
        }

        // 2. Atualização Constante da Interface
        AtualizarInterface();
    }

    private void AtualizarInterface()
    {
        if (player == null) return;

        if (caugeEspecial != null)
        {
            caugeEspecial.fillAmount = player.especialAtual / player.especialMaximo;
            caugeEspecial.color = Color.Lerp(Color.white, Color.red, player.especialAtual / player.especialMaximo);
        }

        if (textoHighScore != null && HighScore < player.score)
        {
            textoHighScore.text = player.score.ToString("F0");
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

        textoPoder.text = player.tiros+"/5";
        textoPods.text = player.listaModulos.Count+"/3";
    }

    public void PausarJogo()
    {
        jogoPausado = true;
        painelPause.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RetomarJogo()
    {
        jogoPausado = false;
        painelPause.SetActive(false);
        Time.timeScale = 1f;
    }

    private void AtualizarPowerUps()
    {
        // Se já tiver uma animação rodando, não reexecuta
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
                        player.GanharPontos(1000); // Se já tiver o máximo de tiros, dá pontos extras
                    }
                    Debug.Log("Efeito ativado: ESPADA");
                });
            }
            // 2. NAVE (cima = 0, baixo = 1)
            else if (cima == 0 && baixo == 1)
            {
                ExecutarAnimacaoCombinacaoPowerUp(() =>
                {
                    if(player.listaModulos.Count < 3)
                    {
                        player.AdicionarModulo();
                    }
                    else
                    {
                        player.GanharPontos(1000); // Se já tiver o máximo de módulos, dá pontos extras
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
                        // Se o jogador já tiver escudo, dá pontos extras
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

    // Recebe uma ação (System.Action) com o efeito que deve ser executado ao final
    private void ExecutarAnimacaoCombinacaoPowerUp(System.Action aoConcluirEfeito)
    {
        processandoAnimacaoPowerUp = true;

        // Limpa animações anteriores nos elementos de UI
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

        // 3. BRILHO DE IMPACTO
        Color corOriginalCima = imagemCimaUI.color;
        Color corOriginalBaixo = imagemBaixoUI.color;

        sequenciaPowerUp.Append(imagemCimaUI.DOColor(Color.yellow, duracaoBrilho * 0.5f));
        sequenciaPowerUp.Join(imagemBaixoUI.DOColor(Color.yellow, duracaoBrilho * 0.5f));

        sequenciaPowerUp.Append(imagemCimaUI.DOColor(corOriginalCima, duracaoBrilho * 0.5f));
        sequenciaPowerUp.Join(imagemBaixoUI.DOColor(corOriginalBaixo, duracaoBrilho * 0.5f));

        // 4. CONCLUSÃO DA ANIMAÇÃO
        sequenciaPowerUp.OnComplete(() =>
        {
            // Reseta posições
            imagemCimaUI.transform.localPosition = posicaoOriginalCima;
            imagemBaixoUI.transform.localPosition = posicaoOriginalBaixo;

            //Reseta escala
            imagemCimaUI.transform.localScale = Vector3.one;
            imagemBaixoUI.transform.localScale = Vector3.one;

            // Limpa os sprites da UI
            imagemCimaUI.sprite = imagemCimaNula;
            imagemBaixoUI.sprite = imagemBaixoNula;

            // Reseta as variáveis de controle
            cima = -1;
            baixo = -1;

            // EXECUTA O EFEITO ESPECÍFICO PASSADO
            aoConcluirEfeito?.Invoke();

            // Libera para novas animações
            processandoAnimacaoPowerUp = false;
        });
    }
}