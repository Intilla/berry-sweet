using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager I;

    [Header("Scene Names")]
    public string mainMenuScene;
    public string gameScene;
    public string highscoreScene;

    void Awake()
    {
        if (I == null)
            I = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
        Time.timeScale = 1f; 
    }

    public void StartGame()
    {
        SceneManager.LoadScene(gameScene);
        Time.timeScale = 1f;
    }

public void LoadHighscore()
{
    var scene = SceneManager.GetSceneByName(highscoreScene);
    if (scene.isLoaded)
    {
    SceneManager.UnloadSceneAsync(highscoreScene);
    }
    else
    {
    SceneManager.LoadScene(highscoreScene, LoadSceneMode.Additive);
    }
}
    public void QuitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void RestartCurrent()
    {
        Scene active = SceneManager.GetActiveScene();
        SceneManager.LoadScene(active.name);
        Time.timeScale = 1f;
    }
}
