using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [System.Serializable]
    private class AudioChannel
    {
        public AudioSource introSource;
        public AudioSource loopSource;
        public Coroutine rotinaTransicao;
        public float targetVolume = 1f;

        public void Inicializar(GameObject holder, string nomeCanal)
        {
            GameObject objIntro = new GameObject($"AudioSource_{nomeCanal}_Intro");
            objIntro.transform.SetParent(holder.transform);
            introSource = objIntro.AddComponent<AudioSource>();
            introSource.loop = false;
            introSource.playOnAwake = false;

            GameObject objLoop = new GameObject($"AudioSource_{nomeCanal}_Loop");
            objLoop.transform.SetParent(holder.transform);
            loopSource = objLoop.AddComponent<AudioSource>();
            loopSource.loop = true;
            loopSource.playOnAwake = false;
        }

        public void DefinirPitch(float pitch)
        {
            if (introSource != null && !Mathf.Approximately(introSource.pitch, pitch))
                introSource.pitch = pitch;

            if (loopSource != null && !Mathf.Approximately(loopSource.pitch, pitch))
                loopSource.pitch = pitch;
        }

        public void PararEPararTweens(MonoBehaviour owner)
        {
            if (rotinaTransicao != null && owner != null)
            {
                owner.StopCoroutine(rotinaTransicao);
                rotinaTransicao = null;
            }

            if (introSource != null)
            {
                introSource.DOKill();
                introSource.Stop();
            }
            if (loopSource != null)
            {
                loopSource.DOKill();
                loopSource.Stop();
            }
        }

        public bool EstaTocando()
        {
            return (introSource != null && introSource.isPlaying) || (loopSource != null && loopSource.isPlaying);
        }
    }

    [Header("Música Inicial")]
    [SerializeField] private bool tocarAoIniciar = true;
    [Tooltip("Clip da introdução da música do menu/fase inicial (opcional)")]
    [SerializeField] private AudioClip introMusicaInicial;
    [Tooltip("Clip do loop principal da música do menu/fase inicial")]
    [SerializeField] private AudioClip loopMusicaInicial;

    [Header("Controle de Volume e Fade")]
    [Range(0f, 1f)][SerializeField] private float volumeMaximo = 1.0f;
    [SerializeField] private float duracaoFadePadrao = 1.5f;

    private AudioChannel canalA = new AudioChannel();
    private AudioChannel canalB = new AudioChannel();

    private AudioChannel canalAtivo;
    private AudioChannel canalInativo;

    private float pitchAnterior = 1.0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InicializarCanais();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (tocarAoIniciar && (introMusicaInicial != null || loopMusicaInicial != null))
        {
            TrocarMusica(introMusicaInicial, loopMusicaInicial, 0f, 0.1f);
        }
    }

    private void InicializarCanais()
    {
        canalA.Inicializar(gameObject, "CanalA");
        canalB.Inicializar(gameObject, "CanalB");

        canalAtivo = canalA;
        canalInativo = canalB;
    }

    private void Update()
    {
        float pitchAtual = Time.timeScale;

        if (!Mathf.Approximately(pitchAnterior, pitchAtual))
        {
            pitchAnterior = pitchAtual;
            canalAtivo?.DefinirPitch(pitchAtual);
            canalInativo?.DefinirPitch(pitchAtual);
        }
    }

    /// <summary>
    /// Troca para uma nova música composta por INTRO + LOOP com controle individual de volume.
    /// </summary>
    /// <param name="introClip">AudioClip da introdução (opcional).</param>
    /// <param name="loopClip">AudioClip do loop (opcional).</param>
    /// <param name="duracaoFade">Duração do fade em segundos. Se -1, usa o valor padrão.</param>
    /// <param name="volume">Volume individual da faixa (0 a 1). Padrão = 1.</param>
    public void TrocarMusica(AudioClip introClip, AudioClip loopClip, float duracaoFade = -1f, float volume = 1f)
    {
        string nomeIntro = introClip != null ? introClip.name : "NENHUM";
        string nomeLoop = loopClip != null ? loopClip.name : "NENHUM";

        bool mesmoIntro = canalAtivo.introSource != null && canalAtivo.introSource.clip == introClip;
        bool mesmoLoop = canalAtivo.loopSource != null && canalAtivo.loopSource.clip == loopClip;

        if (mesmoIntro && mesmoLoop && canalAtivo.EstaTocando())
        {
            Debug.LogWarning($"[MusicManager] Troca IGNORADA: A música (Intro: {nomeIntro} | Loop: {nomeLoop}) já está tocando.");
            return;
        }

        float tempoFade = duracaoFade >= 0f ? duracaoFade : duracaoFadePadrao;
        float volumeFinal = Mathf.Clamp01(volume) * volumeMaximo;

        Debug.Log($"<color=#33FF88>[MusicManager] TROCAR MÚSICA</color> | Intro: <b>{nomeIntro}</b> | Loop: <b>{nomeLoop}</b> | Fade: <b>{tempoFade}s</b> | Vol: <b>{volumeFinal:F2}</b>");

        // 1. Prepara o Fade Out do Canal Ativo Antigo
        canalAtivo.PararEPararTweens(this);

        if (tempoFade > 0f)
        {
            canalAtivo.introSource.DOFade(0f, tempoFade).SetUpdate(true).OnComplete(() => canalAtivo.introSource.Stop());
            canalAtivo.loopSource.DOFade(0f, tempoFade).SetUpdate(true).OnComplete(() => canalAtivo.loopSource.Stop());
        }
        else
        {
            canalAtivo.introSource.Stop();
            canalAtivo.loopSource.Stop();
        }

        // 2. Prepara e Agenda o Novo Canal (Inativo)
        canalInativo.PararEPararTweens(this);
        canalInativo.DefinirPitch(Time.timeScale);
        canalInativo.targetVolume = volumeFinal;

        double tempoBufferDSP = 0.08;
        double dspTimeInicio = AudioSettings.dspTime + tempoBufferDSP;

        if (introClip != null)
        {
            introClip.LoadAudioData();
            double duracaoIntro = introClip.length;

            canalInativo.introSource.clip = introClip;
            canalInativo.introSource.volume = tempoFade > 0f ? 0f : volumeFinal;
            canalInativo.introSource.PlayScheduled(dspTimeInicio);

            if (tempoFade > 0f)
            {
                canalInativo.introSource.DOFade(volumeFinal, tempoFade).SetUpdate(true);
            }

            if (loopClip != null)
            {
                loopClip.LoadAudioData();

                canalInativo.loopSource.clip = loopClip;
                canalInativo.loopSource.volume = volumeFinal;
                canalInativo.loopSource.loop = true;
                canalInativo.loopSource.PlayScheduled(dspTimeInicio + duracaoIntro);

                canalInativo.rotinaTransicao = StartCoroutine(MonitorarTransicaoIntroLoop(canalInativo, (float)duracaoIntro, tempoBufferDSP));
            }
        }
        else if (loopClip != null)
        {
            loopClip.LoadAudioData();

            canalInativo.loopSource.clip = loopClip;
            canalInativo.loopSource.volume = tempoFade > 0f ? 0f : volumeFinal;
            canalInativo.loopSource.loop = true;
            canalInativo.loopSource.PlayScheduled(dspTimeInicio);

            if (tempoFade > 0f)
            {
                canalInativo.loopSource.DOFade(volumeFinal, tempoFade).SetUpdate(true);
            }
        }

        // 3. Inverte os canais
        AudioChannel temp = canalAtivo;
        canalAtivo = canalInativo;
        canalInativo = temp;
    }

    /// <summary>
    /// Sobrecarga para músicas sem introdução (Loop único) com opção de volume.
    /// </summary>
    public void TrocarMusica(AudioClip loopClip, float duracaoFade = -1f, float volume = 1f)
    {
        TrocarMusica(null, loopClip, duracaoFade, volume);
    }

    private IEnumerator MonitorarTransicaoIntroLoop(AudioChannel canal, float duracaoIntro, double tempoBufferDSP)
    {
        yield return new WaitForSecondsRealtime((float)tempoBufferDSP + 0.05f);

        while (canal != null && canal.introSource != null && canal.introSource.isPlaying)
        {
            if (canal.introSource.time >= (duracaoIntro - 0.15f))
            {
                break;
            }
            yield return null;
        }

        if (canal != null && canal.loopSource != null && !canal.loopSource.isPlaying && canal.loopSource.clip != null)
        {
            Debug.Log($"[MusicManager] Transição para o Loop realizada: '{canal.loopSource.clip.name}'.");
            canal.loopSource.volume = canal.targetVolume;
            canal.loopSource.Play();
        }
    }

    public void PararMusica(float duracaoFade = -1f)
    {
        float tempoFade = duracaoFade >= 0f ? duracaoFade : duracaoFadePadrao;

        canalAtivo.PararEPararTweens(this);

        if (tempoFade > 0f)
        {
            canalAtivo.introSource.DOFade(0f, tempoFade).SetUpdate(true).OnComplete(() => canalAtivo.introSource.Stop());
            canalAtivo.loopSource.DOFade(0f, tempoFade).SetUpdate(true).OnComplete(() => canalAtivo.loopSource.Stop());
        }
        else
        {
            canalAtivo.introSource.Stop();
            canalAtivo.loopSource.Stop();
        }
    }

    public void DefinirVolumeMaximo(float novoVolume)
    {
        volumeMaximo = Mathf.Clamp01(novoVolume);

        if (canalAtivo.introSource.isPlaying) canalAtivo.introSource.volume = canalAtivo.targetVolume;
        if (canalAtivo.loopSource.isPlaying) canalAtivo.loopSource.volume = canalAtivo.targetVolume;
    }
}