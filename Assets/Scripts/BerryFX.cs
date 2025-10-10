using System.Collections;
using UnityEngine;

public static class BerryFX
{
    public static void PopAt(Transform t)
    {
        t.gameObject.GetComponent<MonoBehaviour>()?.StartCoroutine(PopCo(t));
    }
    static IEnumerator PopCo(Transform t)
    {
        Vector3 start = t.localScale;
        Vector3 big = start * 1.15f;
        float a=0f; while (a<1f) { a+=Time.deltaTime*10f; t.localScale = Vector3.Lerp(big, start, a); yield return null; }
        t.localScale = start;
    }
}
