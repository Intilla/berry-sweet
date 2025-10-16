using UnityEngine;
using UnityEngine.SceneManagement;

public class BackOrCloseButton : MonoBehaviour
{
    public string menuSceneName = "MainMenu";
    public string highscoreSceneName = "HighscoreScene";

    public void BackOrClose()
    {
        if (SceneManager.GetSceneByName(highscoreSceneName).isLoaded)
            SceneManager.UnloadSceneAsync(highscoreSceneName);
        else
            SceneManager.LoadScene(menuSceneName);
    }
}
