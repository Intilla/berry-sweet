using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Text;

public class HighscoreManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text resultText;
    public TMP_InputField nameInput;
    public TMP_Text leaderboardText;
    public Button submitButton;

    private int totalBerries;
    private string basketInfo;

    [System.Serializable]
    public class HighscoreEntry
    {
        public string name;
        public int berries;
    }

    private List<HighscoreEntry> highscores = new();

    private const string PREF_KEY = "LocalHighscores";

    void Start()
    {
        basketInfo = PlayerPrefs.GetString("BerriesPerBasket", "");

        if (string.IsNullOrEmpty(basketInfo))
            basketInfo = "0";

        string[] basketArray = basketInfo.Split(',');
        int filled = basketArray.Length;
        totalBerries = 0;

        foreach (var b in basketArray)
            if (int.TryParse(b, out int berries))
                totalBerries += berries;

        resultText.text =
            $"Game Over!\n\n" +
            $"Filled Baskets: {filled}\n" +
            $"Total Berries: {totalBerries}";

        submitButton.onClick.AddListener(OnSubmit);

        LoadHighscores();
        DisplayHighscores();
    }

    void OnSubmit()
    {
        string playerName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(playerName))
            playerName = "Anonymous";

        AddHighscore(playerName, totalBerries);
        DisplayHighscores();

        submitButton.interactable = false;
        leaderboardText.text = "Saved score!\n\n" + leaderboardText.text;
    }

    void AddHighscore(string name, int berries)
    {
        highscores.Add(new HighscoreEntry { name = name, berries = berries });
        highscores.Sort((a, b) => b.berries.CompareTo(a.berries)); 

        if (highscores.Count > 10)
            highscores = highscores.GetRange(0, 10);

        SaveHighscores();
    }

    void SaveHighscores()
    {
        StringBuilder sb = new();
        foreach (var entry in highscores)
            sb.AppendLine($"{entry.name}|{entry.berries}");

        PlayerPrefs.SetString(PREF_KEY, sb.ToString());
        PlayerPrefs.Save();
    }

    void LoadHighscores()
    {
        highscores.Clear();

        string raw = PlayerPrefs.GetString(PREF_KEY, "");
        if (string.IsNullOrEmpty(raw)) return;

        string[] lines = raw.Split('\n');
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] parts = line.Split('|');
            if (parts.Length < 2) continue;
            if (int.TryParse(parts[1], out int berries))
                highscores.Add(new HighscoreEntry { name = parts[0], berries = berries });
        }
    }

    void DisplayHighscores()
    {
        if (highscores.Count == 0)
        {
            leaderboardText.text = "No highscores yet!";
            return;
        }

        StringBuilder sb = new();
        for (int i = 0; i < highscores.Count; i++)
        {
            var e = highscores[i];
            sb.AppendLine($"{i + 1}. {e.name} — {e.berries} berries");
        }

        leaderboardText.text = "Highscores:\n\n" + sb.ToString();
    }
}
