using UnityEngine;
using TMPro;
using System.Collections;

public class CoinPopupUI : MonoBehaviour
{
    [Header("References")]
    public TMP_Text popupText;

    [Header("Animation Settings")]
    public float floatDistance = 50f;
    public float duration = 1.2f;

    private CanvasGroup canvasGroup;
    private Vector3 startPos;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        startPos = transform.localPosition;
    }

public void ShowPopup(string message)
{
    popupText.text = message;

    if (!gameObject.activeInHierarchy)
        gameObject.SetActive(true);

    StopAllCoroutines();
    StartCoroutine(AnimatePopup());
}


    private IEnumerator AnimatePopup()
    {
        gameObject.SetActive(true);
        canvasGroup.alpha = 0;
        transform.localPosition = startPos;

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float normalized = t / duration;

            // fade in then fade out
            canvasGroup.alpha = normalized < 0.5f ? normalized * 2f : (1f - normalized) * 2f;

            // float upward
            transform.localPosition = startPos + Vector3.up * (floatDistance * normalized);

            yield return null;
        }

        gameObject.SetActive(false);
    }
}
