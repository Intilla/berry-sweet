using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BasketsUI : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite fullBasket;
    public Sprite emptyBasket;

    [Header("Setup")]
    public int maxBaskets = 3;
    public Vector2 BasketSize = new Vector2(16, 16);
    public Vector2 textOffset = new Vector2(0, -10); 

    private readonly List<Image> Baskets = new List<Image>();
    private readonly List<TextMeshProUGUI> BasketTexts = new List<TextMeshProUGUI>();

    void OnEnable()
    {
        BuildIfNeeded();

        StartCoroutine(DelayedInit());
    }

    System.Collections.IEnumerator DelayedInit()
    {
        yield return null;

        if (BasketManager.I)
        {
            BasketManager.I.OnBasketsChanged.AddListener(UpdateBaskets);

int filled = BasketManager.I.player 
    ? BasketManager.I.player.basketsFilled 
    : 0;

UpdateBaskets(filled);

        }
    }

    void OnDisable()
    {
        if (BasketManager.I)
            BasketManager.I.OnBasketsChanged.RemoveListener(UpdateBaskets);
    }

    void BuildIfNeeded()
    {
        if (Baskets.Count == maxBaskets) return;

        foreach (Transform child in transform)
            Destroy(child.gameObject);

        Baskets.Clear();
        BasketTexts.Clear();

        for (int i = 0; i < maxBaskets; i++)
        {
            var go = new GameObject("Basket_" + i, typeof(Image));
            go.transform.SetParent(transform, false);

            var img = go.GetComponent<Image>();
            img.sprite = emptyBasket;
            var rt = img.rectTransform;
            rt.sizeDelta = BasketSize;

            Baskets.Add(img);

            var textGO = new GameObject("BasketText_" + i, typeof(TextMeshProUGUI));
            textGO.transform.SetParent(go.transform, false);

            var text = textGO.GetComponent<TextMeshProUGUI>();
            text.text = "";
            text.fontSize = 20;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            var textRT = text.rectTransform;
            textRT.anchoredPosition = textOffset;

            BasketTexts.Add(text);
        }
    }

    void UpdateBaskets(int filled)
    {
        var manager = BasketManager.I;

        for (int i = 0; i < Baskets.Count; i++)
        {
            bool isFilled = i < filled;
            Baskets[i].sprite = isFilled ? fullBasket : emptyBasket;

            if (isFilled && manager != null && i < manager.berriesPerBasket.Count)
                BasketTexts[i].text = manager.berriesPerBasket[i].ToString();
            else
                BasketTexts[i].text = "";
        }
    }
}
