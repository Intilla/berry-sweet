using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LivesUI : MonoBehaviour
{
    [Header("References")]
    public PlayerCharacter player;
    public Image[] heartImages;     // assign 3 Image components in order (left → right)
    public Sprite fullHeart;
    public Sprite emptyHeart;

    [Header("Animation")]
    public float popScale = 1.2f;   // how big the pop gets
    public float popSpeed = 8f;     // how fast it pops

    private int lastLives = -1;
    private Coroutine updateRoutine;

    void Awake()
    {
        if (!player)
            player = FindFirstObjectByType<PlayerCharacter>();
    }

    void OnEnable()
    {
        if (player)
        {
            if (updateRoutine != null)
                StopCoroutine(updateRoutine);

            updateRoutine = StartCoroutine(UpdateHeartsLoop());
        }
    }

    void OnDisable()
    {
        if (updateRoutine != null)
            StopCoroutine(updateRoutine);
    }

    IEnumerator UpdateHeartsLoop()
    {
        while (enabled && player)
        {
            if (player.lives != lastLives)
            {
                lastLives = player.lives;
                UpdateHeartSprites();
                StartCoroutine(PopHearts());
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    void UpdateHeartSprites()
    {
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (!heartImages[i]) continue;
            heartImages[i].sprite = (i < player.lives) ? fullHeart : emptyHeart;
            heartImages[i].enabled = true;
        }
    }

    IEnumerator PopHearts()
    {
        float a = 0f;
        while (a < 1f)
        {
            a += Time.unscaledDeltaTime * popSpeed;
            float scale = Mathf.Lerp(popScale, 1f, a);

            foreach (var img in heartImages)
            {
                if (img)
                    img.transform.localScale = Vector3.one * scale;
            }

            yield return null;
        }

        foreach (var img in heartImages)
        {
            if (img)
                img.transform.localScale = Vector3.one;
        }
    }
}
