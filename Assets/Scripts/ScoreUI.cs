using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public enum ScoreMode
    {
        GlobalScore,
        CarriedBerries
    }

    [Header("Setup")]
    public TextMeshProUGUI label;
    public ScoreMode mode = ScoreMode.GlobalScore; // choose in Inspector
    public PlayerCharacter player; // only used for berry mode

    void Awake()
    {
        if (!label) label = GetComponent<TextMeshProUGUI>();
        if (!player && mode == ScoreMode.CarriedBerries)
            player = FindFirstObjectByType<PlayerCharacter>();
    }

    void OnEnable()
    {
        // Initialize text
        RefreshLabel();

        // Subscribe to relevant events
        if (mode == ScoreMode.GlobalScore && ScoreManager.I)
            ScoreManager.I.OnScoreChanged.AddListener(OnScoreChanged);

        else if (mode == ScoreMode.CarriedBerries && player)
            player.OnBerriesChanged.AddListener(OnBerriesChanged);
    }

    void OnDisable()
    {
        if (mode == ScoreMode.GlobalScore && ScoreManager.I)
            ScoreManager.I.OnScoreChanged.RemoveListener(OnScoreChanged);

        else if (mode == ScoreMode.CarriedBerries && player)
            player.OnBerriesChanged.RemoveListener(OnBerriesChanged);
    }

    // Update when global score changes
    void OnScoreChanged(int value)
    {
        label.text = $"{value}";
        Pop();
    }

    // Update when carried berry count changes
    void OnBerriesChanged(int value)
    {
        label.text = $"{value}";
        Pop();
    }

    // Initial setup
    void RefreshLabel()
    {
        if (mode == ScoreMode.GlobalScore)
            label.text = $"{(ScoreManager.I ? ScoreManager.I.Score : 0)}";
        else if (mode == ScoreMode.CarriedBerries && player)
            label.text = $"{player.berriesCarried}";
    }

    // Little bounce animation
    void Pop()
    {
        StopAllCoroutines();
        StartCoroutine(PopRoutine());
    }

    System.Collections.IEnumerator PopRoutine()
    {
        var t = label.transform;
        var s0 = t.localScale;
        var s1 = s0 * 1.1f;
        float a = 0;
        while (a < 1)
        {
            a += Time.unscaledDeltaTime * 10f;
            t.localScale = Vector3.Lerp(s1, s0, a);
            yield return null;
        }
        t.localScale = s0;
    }
}
