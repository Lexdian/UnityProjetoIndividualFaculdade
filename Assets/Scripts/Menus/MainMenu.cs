using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "MainGame";
    public string leaderboardSceneName = "LeaderBoard";
    public string achievementsSceneName = "Achivements";
    public void ReturnToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(gameSceneName);
    }
    public void OpenLeaderboard()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(leaderboardSceneName);
    }
    public void OpenAchievements()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(achievementsSceneName);
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
