using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("Clips de Áudio")]
    [Tooltip("Som tocado ao passar o mouse por cima do botão.")]
    [SerializeField] private AudioClip hoverSound;

    [Tooltip("Som tocado ao clicar no botão (opcional).")]
    [SerializeField] private AudioClip clickSound;

    [Header("Configurações de Áudio")]
    [Range(0f, 1f)][SerializeField] private float volume = 0.7f;

    [Tooltip("AudioSource para tocar o som (se deixado nulo, busca ou cria um automaticamente).")]
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        GarantirAudioSource();
    }

    private void GarantirAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        audioSource.playOnAwake = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TocarSom(hoverSound);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        TocarSom(clickSound);
    }

    private void TocarSom(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }
}