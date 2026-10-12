using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;
    private AudioSource AudioSourceDaMusica;

    void Awake()
    {
        // Sistema de Singleton para garantir que só exista UMA música tocando
        if (instance == null)
        {
            instance = this;
            AudioSourceDaMusica = GetComponent<AudioSource>();
            if (AudioSourceDaMusica == null)
            {
                Debug.LogError("MusicManager precisa de um AudioSource para funcionar!");
            }
            else
            {
                AudioSourceDaMusica.playOnAwake = true;
                AudioSourceDaMusica.loop = true;
                AudioSourceDaMusica.pitch = Time.timeScale; // Garante que a música siga a velocidade do jogo
            }
            // Faz com que este objeto sobreviva à troca de cenas
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Se já existir um MusicManager em outra cena, destrói o novo para não duplicar o som
            Destroy(gameObject);
        }
    }
    // Coloque isso no Update do script que toca a sua música de fundo
    void Update()
    {
        // A música vai seguir em tempo real qualquer alteração de câmera lenta do jogo
        AudioSourceDaMusica.pitch = Time.timeScale;
    }
}