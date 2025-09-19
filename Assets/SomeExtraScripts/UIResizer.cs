using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIResizer : MonoBehaviour
{
    public RectTransform targetUI;
    public Vector2 targetSize = new Vector2(300f, 300f);
    public float resizeDuration = 0.5f;
    public bool playOnStart = true;

    private Vector2 originalSize;

    void Start()
    {
        if (targetUI != null)
        {
            originalSize = targetUI.sizeDelta;

            if (playOnStart)
            {
                StartCoroutine(ResizeUI(targetUI, originalSize, targetSize, resizeDuration));
            }
        }
    }

    public void PlayResize()
    {
        if (targetUI != null)
        {
            StopAllCoroutines();
            StartCoroutine(ResizeUI(targetUI, targetUI.sizeDelta, targetSize, resizeDuration));
        }
    }

    IEnumerator ResizeUI(RectTransform rect, Vector2 fromSize, Vector2 toSize, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            rect.sizeDelta = Vector2.Lerp(fromSize, toSize, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        rect.sizeDelta = toSize;
    }
}