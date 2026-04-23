using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "MainGame";
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }
    public void QuitGame()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }
}
