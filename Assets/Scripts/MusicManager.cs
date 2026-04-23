using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    void Awake()
    {
        // Sistema de Singleton para garantir que só exista UMA música tocando
        if (instance == null)
        {
            instance = this;
            // Faz com que este objeto sobreviva à troca de cenas
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Se já existir um MusicManager em outra cena, destrói o novo para não duplicar o som
            Destroy(gameObject);
        }
    }
}