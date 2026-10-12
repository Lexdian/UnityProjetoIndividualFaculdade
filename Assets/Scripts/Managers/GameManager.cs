using System.Threading.Tasks;
using Unity.Services.Leaderboards;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            return instance;
        }
    }

    [SerializeField]
    private int highScore = 0;
    public int HighScore
    {
        get { return highScore; }
        set
        {
            if (value > highScore)
            {
                highScore = value;
            }
            currentScore = value;
        }
    }

    // Exibe no Inspector mantendo o 'private set' para outros scripts
    [field: SerializeField]
    public int currentScore { get; private set; } = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}