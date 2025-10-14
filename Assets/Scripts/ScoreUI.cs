using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreUI : MonoBehaviour
{
    [Header("References")]
    public PlayerCharacter player;

    [Header("Labels")]
    public TextMeshProUGUI berriesLabel;
    public TextMeshProUGUI coinsLabel;

    [Header("Basket Sprite (optional)")]
    public Image basketImage;
    public Sprite emptyBasketSprite;
    public Sprite fullBasketSprite;

    private Coroutine coinsCoroutine;

    private float lastPopTime;
    private float popCooldown = 0.2f;

    private Vector3 originalBerriesScale;
    private Vector3 originalCoinsScale;

    void Awake()
    {
        if (!player)
            player = FindFirstObjectByType<PlayerCharacter>();

        if (berriesLabel)
            originalBerriesScale = berriesLabel.transform.localScale;

        if (coinsLabel)
            originalCoinsScale = coinsLabel.transform.localScale;
    }

    void OnEnable()
    {
        RefreshLabels();

        if (player)
        {
            player.OnBerriesChanged.AddListener(OnBerriesChanged);
            coinsCoroutine = StartCoroutine(UpdateCoinsLoop());
        }
    }

    void OnDisable()
    {
        if (player)
            player.OnBerriesChanged.RemoveListener(OnBerriesChanged);

        if (coinsCoroutine != null)
            StopCoroutine(coinsCoroutine);
    }

    void OnBerriesChanged(int value)
    {
        if (berriesLabel)
        {
            berriesLabel.text = $"{value}";
            TryPop(berriesLabel.transform, originalBerriesScale);
        }

        if (basketImage)
        {
            basketImage.sprite = (value > 0) ? fullBasketSprite : emptyBasketSprite;
            basketImage.enabled = true;
        }
    }

    IEnumerator UpdateCoinsLoop()
    {
        int lastCoins = -1;
        while (enabled && player)
        {
            if (player.totalCoins != lastCoins)
            {
                lastCoins = player.totalCoins;

                if (coinsLabel)
                {
                    coinsLabel.text = $"{lastCoins}";
                    TryPop(coinsLabel.transform, originalCoinsScale);
                }
            }
            yield return new WaitForSeconds(0.2f);
        }
    }

    void RefreshLabels()
    {
        if (!player) return;

        if (berriesLabel)
            berriesLabel.text = $"{player.berriesCarried}";
        if (coinsLabel)
            coinsLabel.text = $"{player.totalCoins}";
    }

    void TryPop(Transform target, Vector3 originalScale)
    {
        if (Time.unscaledTime - lastPopTime < popCooldown) return;
        lastPopTime = Time.unscaledTime;
        StartCoroutine(PopRoutine(target, originalScale));
    }

    IEnumerator PopRoutine(Transform target, Vector3 baseScale)
    {
        Vector3 s1 = baseScale * 1.1f;
        float duration = 0.15f;

        float a = 0f;
        while (a < 1f)
        {
            a += Time.unscaledDeltaTime / duration;
            target.localScale = Vector3.Lerp(baseScale, s1, a);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(0.05f);

        a = 0f;
        while (a < 1f)
        {
            a += Time.unscaledDeltaTime / duration;
            target.localScale = Vector3.Lerp(s1, baseScale, a);
            yield return null;
        }

        target.localScale = baseScale;
    }
}
