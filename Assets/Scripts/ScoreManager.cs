using UnityEngine;
using UnityEngine.Events;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager I { get; private set; }

    public int Score { get; private set; } = 0;
    public UnityEvent<int> OnScoreChanged = new UnityEvent<int>();

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Add(int amount)
    {
        Score += amount;
        OnScoreChanged.Invoke(Score);
    }

    public void ResetScore(int value = 0)
    {
        Score = value;
        OnScoreChanged.Invoke(Score);
    }
}
