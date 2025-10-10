using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

[System.Serializable]
public class BasketChangeEvent : UnityEvent<int> { }

public class BasketManager : MonoBehaviour
{
    public static BasketManager I;

    [Header("References")]
    public PlayerCharacter player; // 👈 drag your player here in Inspector

    [Header("Basket State")]
    public int maxBaskets = 3;
    public int basketsLost = 0;

    [Header("Data")]
    public List<int> berriesPerBasket = new();

    public BasketChangeEvent OnBasketsChanged = new BasketChangeEvent();

    [Header("Scene Setup")]
    public string highscoreSceneName = "HighscoreScene";

    void Awake() => I = this;

    void Start()
    {
        // make sure we have reference
        if (!player)
            player = FindFirstObjectByType<PlayerCharacter>();

        // initialize UI
        OnBasketsChanged.Invoke(player ? player.basketsFilled : 0);
    }

    public void BankBasket(int berriesBanked)
    {
        if (!player) return;

        if (player.basketsFilled + basketsLost < maxBaskets)
        {
            berriesPerBasket.Add(berriesBanked);
            player.basketsFilled++;

            OnBasketsChanged.Invoke(player.basketsFilled); // ✅ fire update with player's value

            Debug.Log($"Banked basket {player.basketsFilled}/{maxBaskets} with {berriesBanked} berries");

            if (player.basketsFilled >= maxBaskets)
            {
                Debug.Log("🏆 All baskets full! You win!");
                TriggerEndGame();
            }
        }
    }

    public void LoseBasket()
    {
        if (!player) return;

        if (player.basketsFilled + basketsLost < maxBaskets)
        {
            basketsLost++;
            OnBasketsChanged.Invoke(player.basketsFilled);
            Debug.Log($"Lost a basket! Remaining: {maxBaskets - basketsLost - player.basketsFilled}");

            if (basketsLost >= maxBaskets)
            {
                Debug.Log("💀 All baskets lost! Game over!");
                TriggerEndGame();
            }
        }
    }

private void TriggerEndGame()
{
    int finalScore = ScoreManager.I ? ScoreManager.I.Score : 0;
    PlayerPrefs.SetInt("LastScore", finalScore);
    PlayerPrefs.SetString("BerriesPerBasket", string.Join(",", berriesPerBasket));
    PlayerPrefs.Save();

    Time.timeScale = 0f; 
    SceneManager.LoadScene(highscoreSceneName, LoadSceneMode.Additive);
}


    public int GetRemaining() => maxBaskets - player.basketsFilled - basketsLost;
}
