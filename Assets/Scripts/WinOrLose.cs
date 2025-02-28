using UnityEngine;
using UnityEngine.SceneManagement;

public class WinOrLose : MonoBehaviour
{
    public void Win()
    {
        
        LoadNextLevel();
    }

    public void Lose()
    {
        RestartLevel();
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex == 3)
        {
            SceneManager.LoadScene(0);
            return;
        }
        if (SceneManager.sceneCountInBuildSettings == currentSceneIndex + 1)
        {
            SceneManager.LoadScene(0);
            return;
        }
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    private void RestartLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex);
    }
}
