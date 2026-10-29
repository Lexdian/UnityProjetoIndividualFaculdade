using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    [Header("Áudio")]
    public AudioClip somTiro;
    public AudioClip somAtivarEspecial;
    public AudioClip somGraze;
    public AudioClip somMorte;
    public AudioClip somEscudoQuebrado;
    public AudioClip somSacrificio;
    [Range(0f, 1f)] public float volumeTiro = 0.5f;
    [Range(0f, 1f)] public float volumeEspecial = 0.8f;
    [Range(0f, 1f)] public float volumeGraze = 0.4f;
    [Range(0f, 1f)] public float volumeMorte = 0.4f;
    [Range(0f, 1f)] public float volumeEscudoQuebrado = 0.8f;
    [Range(0f, 1f)] public float volumeSacrificio = 0.8f;
    private AudioSource audioSource;

    [Header("Trilha Sonora de Morte / Game Over")]
    [Tooltip("Clip da introdução da música de Game Over (opcional)")]
    [SerializeField] private AudioClip introMusicaMorte;
    [Tooltip("Clip do loop da música de Game Over")]
    [SerializeField] private AudioClip loopMusicaMorte;
    [Range(0f, 1f)][SerializeField] private float volumeMusicaMorte = 1.0f;

    [Header("Vozes do Anunciador Arcade")]
    public AudioClip vocOverdrive;
    public AudioClip vocShieldBroken;
    public AudioClip vocJackpot;
    [Range(0f, 1f)] public float volumeAnunciador = 0.8f;

    float moveHorizontal;
    float moveVertical;
    public float velocidade = 5f;

    [Header("Configurações de Dano/Invulnerabilidade")]
    [SerializeField] private float tempoInvulneravel = 2f;
    [SerializeField] private float intervaloPisca = 0.1f;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private bool isInvulnerable = false;

    [Header("Configurações de Tiro")]
    public GameObject bulletPrefab;
    Rigidbody2D rb;
    public Vector2 bulletSpawnOffset = new Vector2(0, 1f);
    public float fireRateBase = 0.5f;
    public float fireRateMinimo = 0.1f;
    private float nextFireTime = 0f;

    [Header("Efeitos Visuais do Tiro")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private float tempoVidaMuzzleFlash = 0.08f;

    [Header("Consumo de Vida por Bônus")]
    [SerializeField] private float aumentoMultiplicadorPorVida = 0.5f;
    private float bonusMultiplicadorBase = 0f;

    private Animator anim;
    private BulletPool pool;
    [SerializeField] private GameObject hitboxVisual;
    private SpriteRenderer hitboxRenderer;
    private Camera mainCam;

    [SerializeField] private Color corA = Color.white;
    [SerializeField] private Color corB = Color.red;
    [SerializeField] private float velocidadeOscilacao = 5f;

    public int score = 0;
    public float scoreMultiplier = 1;
    private int nextVidaScoreThreshold = 10000;
    private bool canMove = true;
    public bool temEscudo = false;
    public GameObject escudo;
    public GameObject partesEscudo;
    public int vidas = 3;

    [Header("Configurações do Padrão de Tiro")]
    [SerializeField] private float espacamentoHorizontal = 0.25f;
    [SerializeField] private float curvaturaArco = 0.15f;
    public int tiros = 1;

    [Header("Sistema de Graze & Especial")]
    public float especialMaximo = 100f;
    public float especialAtual = 0f;
    public float ganhoGraze = 5f;
    public float perdaEspecialPorSegundo = 0.01f;
    public float tempoParaComecarPerder = 1.5f;
    private float timerUltimoGraze;

    [Header("Habilidade Especial (Time Slow)")]
    public float custoAtivacaoMinimo = 20f;
    public float taxaConsumoHabilidade = 15f;
    public float fatorCameraLenta = 0.5f;
    public float velocidadeRecuperacaoTempo = 2f;
    private bool estaUsandoHabilidade = false;
    private bool recuperandoTempo = false;

    [Header("Efeito Visual da Onda (Shockwave)")]
    [SerializeField] private Renderer waveRenderer;
    [SerializeField] private string nomePropriedadeShader = "WaveDistance";
    [SerializeField] private float velocidadeOnda = 10f;
    private Material waveMaterial;
    private float waveDistanceAtual = -1f;

    [Header("Sistema de Dano / Respawn")]
    [SerializeField] private Vector2 posicaoTeleporte = new Vector2(0f, -8f);
    [SerializeField] private Vector2 posicaoAlvoRespawn = new Vector2(0f, -3.5f);
    [SerializeField] private float tempoMoverRespawn = 1.5f;
    [SerializeField] private ParticleSystem particulasDano;

    [Header("Sistema de Módulos (Options)")]
    [SerializeField] private GameObject prefabModulo;
    [SerializeField] private int maxHistoricoPosicoes = 300;
    private System.Collections.Generic.List<Vector3> historicoPosicoes = new System.Collections.Generic.List<Vector3>();
    public System.Collections.Generic.List<OptionModule> listaModulos = new System.Collections.Generic.List<OptionModule>();

    [Header("Sistema de Auto-Coleta (Topo da Tela / POC)")]
    [SerializeField] private float limiteTopoViewport = 0.8f;
    [SerializeField] private float velocidadeAtracaoColetaveis = 16f;
    [SerializeField] private float tempoResetPocCombo = 1.0f;
    private int pocStreak = 0;
    private float timerPocCombo = 0f;

    [Header("Efeito Visual de Sacrifício")]
    [SerializeField] private ParticleSystem particulasSacrificio;
    [SerializeField] private float duracaoHitStopSacrificio = 0.05f;

    private Coroutine damageFlashCoroutine;

    public static PlayerMovement Instance { get; private set; }
    private System.Collections.Generic.List<Collectible> coletaveisAtivos = new System.Collections.Generic.List<Collectible>();

    public bool EmFoco { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        mainCam = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        pool = gameObject.AddComponent<BulletPool>();
        pool.bulletPrefab = bulletPrefab;
        pool.Initialize(50);

        hitboxRenderer = hitboxVisual.GetComponent<SpriteRenderer>();
        hitboxRenderer.enabled = false;

        if (waveRenderer != null)
        {
            waveMaterial = waveRenderer.material;
            waveMaterial.SetFloat(nomePropriedadeShader, -1f);
        }
    }

    public void Moved(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        Vector2 input = context.ReadValue<Vector2>();
        moveHorizontal = input.x;
        moveVertical = input.y;

        if (anim != null)
        {
            anim.SetBool("TurningLeft", moveHorizontal < -0.1f);
            anim.SetBool("TurningRight", moveHorizontal > 0.1f);
        }
    }

    float CalcularFireRate()
    {
        float percentualEspecial = especialAtual / especialMaximo;
        return Mathf.Lerp(fireRateBase, fireRateMinimo, percentualEspecial);
    }

    public void Focus(InputAction.CallbackContext context)
    {
        if (!canMove) return;

        if (context.started || context.performed)
        {
            EmFoco = true;
            velocidade = 3f;
            hitboxRenderer.enabled = true;
        }
        else if (context.canceled)
        {
            EmFoco = false;
            velocidade = 5f;
            hitboxRenderer.enabled = false;
            RecomporHistoricoPosicoes();
        }
    }

    void Update()
    {
        escudo.SetActive(temEscudo);
        if (hitboxRenderer.enabled && canMove)
        {
            float t = Mathf.PingPong(Time.time * velocidadeOscilacao, 1f);
            hitboxRenderer.color = Color.Lerp(corA, corB, t);
        }

        if (Keyboard.current.xKey.wasPressedThisFrame && canMove)
        {
            ConsumirVidaPorMultiplicador();
        }

        float currentFireRate = CalcularFireRate();
        nextFireTime += Time.unscaledDeltaTime;

        if (canMove && Keyboard.current.zKey.isPressed && nextFireTime >= currentFireRate)
        {
            Disparar();
            nextFireTime = 0;
        }

        ControlarHabilidadeEspecial();
        SuavizarRetornoDoTempo();
        AtualizarEfeitoOnda();

        VerificarAtracaoTopoTela();
        AtualizarComboPoc();

        if (!estaUsandoHabilidade)
        {
            float progressoEspecial = (float)Math.Round((especialAtual / especialMaximo), 1);
            scoreMultiplier = Mathf.Round(((1f + bonusMultiplicadorBase) + progressoEspecial) * 10f) / 10f;
        }

        if (score >= nextVidaScoreThreshold)
        {
            vidas++;
            nextVidaScoreThreshold += 50000;
        }

        AtualizarHistoricoPosicoes();
    }

    public void GanharPontos(int pontos)
    {
        float multExato = Mathf.Round(scoreMultiplier * 10f) / 10f;
        int pontosCalculados = Mathf.RoundToInt(pontos * multExato);
        pontosCalculados = Mathf.RoundToInt(pontosCalculados / 10f) * 10;

        Debug.Log($"Ganhou {pontos} pontos base com multiplicador de {multExato:F1}x. Total concedido: {pontosCalculados}");

        if (ScoreBonusManager.Instance != null)
        {
            // Adiciona na fila pendente para ser somado na UI somente após o término do efeito visual/texto flutuante
            ScoreBonusManager.Instance.AdicionarPontosPendentes(pontosCalculados);
        }
        else
        {
            // Fallback caso o ScoreBonusManager não esteja presente na cena
            score += pontosCalculados;
        }
    }

    private void VerificarAtracaoTopoTela()
    {
        if (!canMove || mainCam == null) return;

        Vector3 viewPos = mainCam.WorldToViewportPoint(transform.position);
        if (viewPos.y >= limiteTopoViewport)
        {
            PuxarTodosColetaveis();
        }
    }

    public void RegistrarColetavel(Collectible item)
    {
        if (!coletaveisAtivos.Contains(item))
        {
            coletaveisAtivos.Add(item);
        }
    }

    public void DesregistrarColetavel(Collectible item)
    {
        if (coletaveisAtivos.Contains(item))
        {
            coletaveisAtivos.Remove(item);
        }
    }

    private void PuxarTodosColetaveis()
    {
        for (int i = coletaveisAtivos.Count - 1; i >= 0; i--)
        {
            Collectible item = coletaveisAtivos[i];

            if (item != null && !item.EstaSendoAtraido())
            {
                item.AtrairPara(transform, velocidadeAtracaoColetaveis, true);
            }
        }
    }

    private void AtualizarComboPoc()
    {
        if (pocStreak > 0)
        {
            timerPocCombo -= Time.deltaTime;
            if (timerPocCombo <= 0f)
            {
                pocStreak = 0;
            }
        }
    }

    public void ColetarItemPoc(int valorBase)
    {
        pocStreak++;

        if (pocStreak == 10 && vocJackpot != null && audioSource != null)
        {
            audioSource.PlayOneShot(vocJackpot, volumeAnunciador);
        }

        float t = Mathf.Clamp01((float)(pocStreak - 1) / 9f);
        Color corCombo = Color.Lerp(Color.yellow, Color.red, t);

        float minFontSize = 2.5f;
        float maxFontSize = 5.0f;
        float tamanhoFonte = Mathf.Lerp(minFontSize, maxFontSize, t);

        FloatingTextManager.Show($"x{pocStreak}", transform.position + new Vector3(0f, 0.2f, 0f), corCombo, tamanhoFonte);

        int pontosComCombo = valorBase * pocStreak;
        GanharPontos(pontosComCombo);
        timerPocCombo = tempoResetPocCombo;
    }

    private void ConsumirVidaPorMultiplicador()
    {
        if (vidas > 0)
        {
            vidas--;
            bonusMultiplicadorBase += aumentoMultiplicadorPorVida;

            if (estaUsandoHabilidade)
            {
                scoreMultiplier += (aumentoMultiplicadorPorVida * 2f);
            }

            if (somSacrificio != null && audioSource != null)
            {
                audioSource.PlayOneShot(somSacrificio, volumeSacrificio);
            }

            FloatingTextManager.Show($"SACRIFICE! +{aumentoMultiplicadorPorVida:F1}x", transform.position + new Vector3(0f, 0.5f, 0f), Color.red, 3.5f);

            StartCoroutine(ExecutarHitStop(duracaoHitStopSacrificio));

            if (damageFlashCoroutine != null) StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = StartCoroutine(DamageFlashRoutine());

            if (particulasSacrificio != null)
            {
                particulasSacrificio.Play();
            }

            if (CameraShakeController.Instance != null)
            {
                CameraShakeController.Instance.Shake(0.12f);
            }

            Debug.Log($"Vida consumida! Novo Bônus Base: {bonusMultiplicadorBase} | Vidas restantes: {vidas}");
        }
    }

    private IEnumerator ExecutarHitStop(float duracao)
    {
        float timeScaleAnterior = Time.timeScale;
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(duracao);
        Time.timeScale = timeScaleAnterior;
    }

    void AtivarHabilidade(bool ativar)
    {
        estaUsandoHabilidade = ativar;

        if (ativar)
        {
            scoreMultiplier = scoreMultiplier * 2;
            recuperandoTempo = false;
            Time.timeScale = fatorCameraLenta;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            if (audioSource != null)
            {
                audioSource.pitch = fatorCameraLenta;
            }
        }
        else
        {
            recuperandoTempo = true;
        }
    }

    public void RecomporHistoricoPosicoes()
    {
        historicoPosicoes.Clear();
        int totalPontosNecessarios = maxHistoricoPosicoes;

        for (int i = 0; i < totalPontosNecessarios; i++)
        {
            Vector3 pontoVirtual = transform.position - new Vector3(0f, 0.1f * i, 0f);
            historicoPosicoes.Add(pontoVirtual);
        }
    }

    private void AtualizarHistoricoPosicoes()
    {
        if (EmFoco) return;

        if (historicoPosicoes.Count == 0 || Vector3.Distance(historicoPosicoes[0], transform.position) > 0.01f)
        {
            historicoPosicoes.Insert(0, transform.position);

            if (historicoPosicoes.Count > maxHistoricoPosicoes)
            {
                historicoPosicoes.RemoveAt(historicoPosicoes.Count - 1);
            }
        }
    }

    public Vector3 ObterPosicaoHistorico(int framesAtras)
    {
        if (historicoPosicoes.Count == 0) return transform.position;

        int index = Mathf.Clamp(framesAtras, 0, historicoPosicoes.Count - 1);
        return historicoPosicoes[index];
    }

    void ControlarHabilidadeEspecial()
    {
        if (!canMove) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame && especialAtual >= custoAtivacaoMinimo && !estaUsandoHabilidade && !recuperandoTempo)
        {
            if (somAtivarEspecial != null && audioSource != null)
            {
                audioSource.PlayOneShot(somAtivarEspecial, volumeEspecial);
            }

            if (vocOverdrive != null && audioSource != null)
            {
                audioSource.PlayOneShot(vocOverdrive, volumeAnunciador);
            }

            AtivarHabilidade(true);
        }

        if (estaUsandoHabilidade)
        {
            especialAtual -= taxaConsumoHabilidade * Time.unscaledDeltaTime;

            if (especialAtual <= 0)
            {
                especialAtual = 0;
                AtivarHabilidade(false);
            }
        }
        else if (!recuperandoTempo)
        {
            if (especialAtual > 0 && Time.time > timerUltimoGraze + tempoParaComecarPerder)
            {
                especialAtual -= perdaEspecialPorSegundo * Time.deltaTime;
            }
        }
    }

    void SuavizarRetornoDoTempo()
    {
        if (recuperandoTempo)
        {
            Time.timeScale += velocidadeRecuperacaoTempo * Time.unscaledDeltaTime;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;

            if (audioSource != null)
            {
                audioSource.pitch = Time.timeScale;
            }

            if (Time.timeScale >= 1f)
            {
                Time.timeScale = 1f;
                Time.fixedDeltaTime = 0.02f;

                if (audioSource != null) audioSource.pitch = 1f;

                recuperandoTempo = false;
            }
        }
    }

    void AtualizarEfeitoOnda()
    {
        if (waveMaterial == null) return;

        float alvoWave = estaUsandoHabilidade ? 1f : -0.1f;
        waveDistanceAtual = Mathf.MoveTowards(waveDistanceAtual, alvoWave, velocidadeOnda * Time.unscaledDeltaTime);
        waveMaterial.SetFloat(nomePropriedadeShader, waveDistanceAtual);
    }

    void Disparar()
    {
        int quantidadeTiros = Mathf.Clamp(tiros, 1, 5);

        if (quantidadeTiros == 1)
        {
            InstanciarBala(bulletSpawnOffset);
        }
        else
        {
            for (int i = 0; i < quantidadeTiros; i++)
            {
                float t = (quantidadeTiros > 1) ? Mathf.Lerp(-1f, 1f, (float)i / (quantidadeTiros - 1)) : 0f;
                float offsetX = t * (espacamentoHorizontal * (quantidadeTiros - 1) * 0.5f);
                float offsetY = (1f - (t * t)) * curvaturaArco;

                Vector2 offsetFinal = new Vector2(bulletSpawnOffset.x + offsetX, bulletSpawnOffset.y + offsetY);
                InstanciarBala(offsetFinal);
            }
        }

        TocarSomTiro();

        foreach (var modulo in listaModulos)
        {
            if (modulo != null) modulo.Disparar();
        }
    }

    public void AdicionarModulo()
    {
        if (prefabModulo == null) return;

        GameObject obj = Instantiate(prefabModulo, transform.position, Quaternion.identity);
        OptionModule modulo = obj.GetComponent<OptionModule>();

        if (modulo != null)
        {
            modulo.Inicializar(this, pool, listaModulos.Count);
            listaModulos.Add(modulo);
        }
    }

    private void InstanciarBala(Vector2 offsetLocal)
    {
        Vector3 posicaoSaida = transform.position + (Vector3)offsetLocal;

        GameObject bullet = pool.GetBullet();
        if (bullet != null)
        {
            bullet.transform.position = posicaoSaida;
            BulletMovement bm = bullet.GetComponent<BulletMovement>();
            bm.damage = 1 - (tiros - 1) * 0.125f;
            bm.SetPool(pool);
            bm.SetDirection(Vector2.up);
        }

        if (muzzleFlashPrefab != null)
        {
            GameObject flash = Instantiate(muzzleFlashPrefab, posicaoSaida - new Vector3(0f, 0.2f, 0f), muzzleFlashPrefab.transform.rotation);
            flash.transform.SetParent(transform, true);
            Destroy(flash, tempoVidaMuzzleFlash);
        }
    }

    private void TocarSomTiro()
    {
        if (somTiro != null && audioSource != null)
        {
            audioSource.PlayOneShot(somTiro, volumeTiro);
        }
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        float multiplicadorVelocidadeTempo = 1f / Time.timeScale;
        rb.linearVelocity = new Vector2(moveHorizontal, moveVertical) * velocidade * multiplicadorVelocidadeTempo;
        RestringirAoViewport();
    }

    void RestringirAoViewport()
    {
        Vector3 viewPos = mainCam.WorldToViewportPoint(rb.position);
        viewPos.x = Mathf.Clamp(viewPos.x, 0.05f, 0.95f);
        viewPos.y = Mathf.Clamp(viewPos.y, 0.05f, 0.95f);
        rb.position = mainCam.ViewportToWorldPoint(viewPos);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("EnemyBullet"))
        {
            if (isInvulnerable) return;

            if (temEscudo)
            {
                temEscudo = false;
                partesEscudo.SetActive(true);

                if (somEscudoQuebrado != null && audioSource != null)
                {
                    audioSource.PlayOneShot(somEscudoQuebrado, volumeEscudoQuebrado);
                }

                if (vocShieldBroken != null && audioSource != null)
                {
                    audioSource.PlayOneShot(vocShieldBroken, volumeAnunciador);
                }

                StartCoroutine(EfeitoDanoCo());
                return;
            }

            if (somMorte != null)
                audioSource.PlayOneShot(somMorte, volumeMorte);

            vidas--;
            particulasDano.Play();
            EnemySpawner.Instance.RegistrarMorteJogador();

            if (vidas < 0)
                Death();
            else
                Damaged();
        }
    }

    private void Damaged()
    {
        canMove = false;
        moveHorizontal = 0f;
        moveVertical = 0f;
        rb.linearVelocity = Vector2.zero;

        hitboxRenderer.enabled = false;

        estaUsandoHabilidade = false;
        recuperandoTempo = false;
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        if (audioSource != null) audioSource.pitch = 1f;

        especialAtual = 0f;
        pocStreak = 0;

        rb.position = posicaoTeleporte;

        StartCoroutine(EfeitoDanoCo());
        StartCoroutine(MoverParaPosicaoInicial());
    }

    private IEnumerator EfeitoDanoCo()
    {
        isInvulnerable = true;
        float tempoPassado = 0f;

        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        while (tempoPassado < tempoInvulneravel)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(intervaloPisca);
            tempoPassado += intervaloPisca;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
    }

    private IEnumerator MoverParaPosicaoInicial()
    {
        float tempoDecorrido = 0f;
        Vector2 posicaoInicial = posicaoTeleporte;

        while (tempoDecorrido < tempoMoverRespawn)
        {
            tempoDecorrido += Time.unscaledDeltaTime;
            float taxa = tempoDecorrido / tempoMoverRespawn;
            float taxaSuave = Mathf.SmoothStep(0f, 1f, taxa);

            rb.position = Vector2.Lerp(posicaoInicial, posicaoAlvoRespawn, taxaSuave);
            yield return null;
        }

        rb.position = posicaoAlvoRespawn;
        canMove = true;
    }

    private void Death()
    {
        // Força a consolidação instantânea de todos os pontos pendentes antes de registrar a pontuação final
        if (ScoreBonusManager.Instance != null)
        {
            ScoreBonusManager.Instance.CommitImmediate();
        }

        AchievementManager.Instance.UnlockAchievementsByTarget("score", score);
        GameManager.Instance.HighScore = this.score;
        Debug.Log("Conquista de pontuação desbloqueada!");

        canMove = false;
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (hitboxRenderer != null) hitboxRenderer.enabled = false;

        if (MusicManager.Instance != null && (introMusicaMorte != null || loopMusicaMorte != null))
        {
            MusicManager.Instance.TrocarMusica(introMusicaMorte, loopMusicaMorte, 1.0f, volumeMusicaMorte);
        }

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.IniciarGameOverSequence();
        }
        else
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("GameOver");
        }
    }

    public void RegistrarGraze()
    {
        if (estaUsandoHabilidade || !canMove) return;

        if (somGraze != null && audioSource != null)
        {
            audioSource.PlayOneShot(somGraze, volumeGraze);
        }

        especialAtual += ganhoGraze;
        especialAtual = Mathf.Clamp(especialAtual, 0, especialMaximo);
        timerUltimoGraze = Time.time;
        EnemySpawner.Instance.RegistrarGraze();
    }

    private IEnumerator DamageFlashRoutine()
    {
        if (spriteRenderer != null)
        {
            Color corOriginal = Color.white;
            spriteRenderer.color = Color.red;

            yield return new WaitForSecondsRealtime(0.08f);

            spriteRenderer.color = corOriginal;
        }
    }
}