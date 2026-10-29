using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("Áudio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip vocGameOver;
    [Range(0f, 1f)][SerializeField] private float volumeVoz = 0.8f;

    [Header("Interface e Transição")]
    [Tooltip("CanvasGroup da imagem preta que cobre a tela para fazer o Fade Out")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    [Tooltip("Texto 'GAME OVER' no centro da tela para alinhar perfeitamente com a cena seguinte")]
    [SerializeField] private TextMeshProUGUI textoGameOver;

    [SerializeField] private float duracaoEscurecimento = 2.0f;
    [SerializeField] private float tempoEsperaAposVoz = 0.5f;

    [Header("Configuração de Cena")]
    [SerializeField] private string nomeCenaGameOver = "GameOver";

    private bool jaIniciouGameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
        }

        if (textoGameOver != null)
        {
            textoGameOver.gameObject.SetActive(false);
        }
    }

    public void IniciarGameOverSequence()
    {
        if (jaIniciouGameOver) return;
        jaIniciouGameOver = true;

        StartCoroutine(RoutineGameOver());
    }

    private IEnumerator RoutineGameOver()
    {
        Time.timeScale = 1f;

        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.blocksRaycasts = true;
        }

        // 1. Configura a posição e escala exatas para bater com o ponto de partida do GameOver.cs
        if (textoGameOver != null)
        {
            textoGameOver.transform.localPosition = Vector3.zero;
            textoGameOver.transform.localScale = Vector3.one * 1.5f;
            textoGameOver.text = "GAME OVER";
            textoGameOver.gameObject.SetActive(true);
        }

        // 2. Toca a voz do anunciador ("MISSION FAILED! PILOT DOWN!")
        if (vocGameOver != null && audioSource != null)
        {
            audioSource.PlayOneShot(vocGameOver, volumeVoz);
        }

        // 3. Escurece a tela suavemente via CanvasGroup
        float timer = 0f;
        while (timer < duracaoEscurecimento)
        {
            timer += Time.unscaledDeltaTime;
            if (fadeCanvasGroup != null)
            {
                fadeCanvasGroup.alpha = Mathf.Clamp01(timer / duracaoEscurecimento);
            }
            yield return null;
        }

        if (fadeCanvasGroup != null) fadeCanvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(tempoEsperaAposVoz);

        // 4. Carrega a nova cena onde o GameOver.cs assume a animação do texto
        SceneManager.LoadScene(nomeCenaGameOver);
    }
}