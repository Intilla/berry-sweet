using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine.UI;

public class HighscoreManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField nameInput;
    public TMP_Text leaderboardText;
    public TMP_Text statusText;
    public Button submitButton;

    [Header("Server Settings")]
    public string baseUrl = "https://intilla.se/berry-sweet/"; 
    public string secretKey = "OtterPower123"; 

    private void OnEnable()
    {
        StartCoroutine(FetchHighscores());
    }

    private void Start()
    {
        statusText.text = "Loading leaderboard...";
        submitButton.onClick.AddListener(OnSubmit);

        StartCoroutine(FetchHighscores());
    }

    public void OnSubmit()
    {
        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonymous";

        int money = PlayerPrefs.GetInt("LastMoney", 0);
        if (money <= 0)
        {
            statusText.text = "You have no earnings to submit!";
            return;
        }

        StartCoroutine(UploadScore(playerName, money));
    }

    private IEnumerator UploadScore(string playerName, int money)
    {
        statusText.text = "Uploading...";

        ScoreEntry entry = new ScoreEntry(playerName, money);
        string json = JsonUtility.ToJson(entry);
        string url = baseUrl + "save.php?key=" + secretKey;

        UnityWebRequest www = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Upload failed: " + www.error);
            statusText.text = "Upload failed";
        }
        else
        {
            Debug.Log("Upload success: " + www.downloadHandler.text);
            statusText.text = "Uploaded!";

            StartCoroutine(FetchHighscores());
        }
    }

    private IEnumerator FetchHighscores()
    {
        leaderboardText.text = "Loading highscores...";

        string url = baseUrl + "load.php";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.downloadHandler = new DownloadHandlerBuffer();

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to fetch highscores: " + www.error);
            leaderboardText.text = "Error loading highscores";
            yield break;
        }

        string json = www.downloadHandler.text;
        Debug.Log("Data received: " + json);

        HighscoreList list = JsonUtility.FromJson<HighscoreList>(json);

        if (list == null || list.highscores == null || list.highscores.Count == 0)
        {
            leaderboardText.text = "No highscores yet.";
            statusText.text = "Leaderboard empty";
            yield break;
        }

        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < list.highscores.Count; i++)
        {
            var entry = list.highscores[i];
            sb.AppendLine($"{i + 1}. {entry.name} — {entry.score} coins");
        }

        leaderboardText.text = sb.ToString();
        statusText.text = "Leaderboard updated";
    }

    [System.Serializable]
    public class ScoreEntry
    {
        public string name;
        public int score;

        public ScoreEntry(string name, int score)
        {
            this.name = name;
            this.score = score;
        }
    }

    [System.Serializable]
    public class HighscoreList
    {
        public List<ScoreEntry> highscores;
    }
}
