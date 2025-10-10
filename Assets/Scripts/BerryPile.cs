using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;  
using URandom = UnityEngine.Random; 

public class BerryPile : MonoBehaviour
{
    [Header("Visuals (pips)")]
    public Sprite berrySprite;                
    [Range(0.2f, 1f)] public float pipScale = 0.6f;
    public float pipZStep = 0.001f;

    [Header("Tint per type")]
    public Color blueberryTint = new(0.30f, 0.45f, 0.95f);
    public Color lingonTint = new(0.82f, 0.17f, 0.20f);

    [Header("State")]
    public string typeId;
    public int berries;
    public Vector2Int countRange;
    public Vector2 regrowRangeSec;

    public Action<BerryPile> OnDepleted;
    public bool hideWhenEmpty = true;

    // runtime
    readonly List<SpriteRenderer> pips = new();
    Collider2D triggerCol;
    bool isRegrowing;

    public void Init(string id, int min, int max)
    {
        typeId = id;
        countRange = new Vector2Int(min, max);
        triggerCol = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();
        BuildIfNeeded();
        RollNewCount();
        UpdateVisual();

        if (triggerCol)
        {
            triggerCol.enabled = true;
            if (triggerCol is BoxCollider2D box) box.isTrigger = true;
        }
        int berryLayer = LayerMask.NameToLayer("Berry");
        if (berryLayer >= 0) gameObject.layer = berryLayer;

    }

[NonSerialized] public string reservedBy; 

public bool IsAvailable(string requester)
{
    return berries > 0 && (string.IsNullOrEmpty(reservedBy) || reservedBy == requester);
}

public bool TryReserve(string requester)
{
    if (!IsAvailable(requester)) return false;
    if (string.IsNullOrEmpty(reservedBy)) reservedBy = requester;
    return reservedBy == requester;
}

public void Unreserve(string requester)
{
    if (reservedBy == requester) reservedBy = null;
}


    public bool TryPick()
    {
        if (isRegrowing) return false;
        if (berries <= 0) return false;

        berries--;
        UpdateVisual();

        if (berries <= 0)
        {
            Debug.Log($"[{name}] emptied → start regrow timer");
            reservedBy = null;
            if (triggerCol) triggerCol.enabled = false;
            if (hideWhenEmpty) SetPipsVisible(false);
            OnDepleted?.Invoke(this);
        }
        return true;
    }

    System.Collections.IEnumerator RegrowCo()
    {
        isRegrowing = true;

        float wait = URandom.Range(regrowRangeSec.x, regrowRangeSec.y);
        float end = Time.realtimeSinceStartup + wait;
        while (Time.realtimeSinceStartup < end)
            yield return null;

        RollNewCount();
        UpdateVisual();
        if (triggerCol) triggerCol.enabled = true;
        isRegrowing = false;

        Debug.Log($"[{name}] regrew → {berries} berries");
    }


    void BuildIfNeeded()
    {
        if (pips.Count > 0) return;

        for (int i = 0; i < 5; i++)
        {
            var go = new GameObject($"Pip_{i}");
            go.transform.SetParent(transform, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = berrySprite;
            var parentSR = GetComponentInChildren<SpriteRenderer>();
            if (parentSR)
            {
                sr.sortingLayerID = parentSR.sortingLayerID;
                sr.sortingOrder = parentSR.sortingOrder + 1 + i;
            }
            go.transform.localScale = Vector3.one * pipScale;
            pips.Add(sr);
        }

        triggerCol = GetComponent<Collider2D>();
    }

    void RollNewCount()
    {
        berries = URandom.Range(countRange.x, countRange.y + 1);
    }

    void UpdateVisual()
    {
        var tint = typeId == "blueberry" ? blueberryTint : lingonTint;

        var positions = LayoutForCount(Mathf.Clamp(berries, 0, 5));

        for (int i = 0; i < pips.Count; i++)
        {
            bool show = i < berries;
            pips[i].enabled = show;
            if (show)
            {
                pips[i].color = tint;
                var pos = positions[i];
                pips[i].transform.localPosition = new Vector3(pos.x, pos.y, -i * pipZStep);
            }
        }

        if (hideWhenEmpty && berries <= 0) SetPipsVisible(false);
    }

    void SetPipsVisible(bool v)
    {
        foreach (var sr in pips) sr.enabled = v;
    }

    static Vector2[] LayoutForCount(int count)
    {
        switch (count)
        {
            case 1: return new[] { new Vector2(0f, 0f) };
            case 2: return new[] { new Vector2(-0.16f, 0f), new Vector2(0.16f, 0f) };
            case 3: return new[] { new Vector2(-0.18f, -0.06f), new Vector2(0.18f, -0.06f), new Vector2(0f, 0.16f) };
            case 4:
                return new[] {
                new Vector2(-0.18f,  0.10f), new Vector2(0.18f,  0.10f),
                new Vector2(-0.18f, -0.12f), new Vector2(0.18f, -0.12f) };
            case 5:
                return new[] {
                new Vector2(-0.20f,  0.12f), new Vector2(0.20f,  0.12f),
                new Vector2(-0.24f, -0.14f), new Vector2(0.24f, -0.14f),
                new Vector2(0f, -0.02f) };
            default: return System.Array.Empty<Vector2>();
        }
    }
}
