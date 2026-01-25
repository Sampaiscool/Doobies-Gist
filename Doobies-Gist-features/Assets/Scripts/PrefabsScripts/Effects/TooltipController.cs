using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TooltipController : MonoBehaviour
{
    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Coroutine animRoutine;

    public void Init(string text, Vector2 hiddenOffset, Vector2 shownOffset, float animDuration)
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Set text
        TMP_Text tooltipText = GetComponentInChildren<TMP_Text>();
        if (tooltipText != null) tooltipText.text = text;

        // Start hidden
        rect.anchoredPosition = hiddenOffset;
        canvasGroup.alpha = 0f;

        // Animate in
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(Animate(hiddenOffset, 0f, shownOffset, 1f, false, animDuration));
    }

    public void Hide(Vector2 hiddenOffset, float animDuration)
    {
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(Animate(rect.anchoredPosition, canvasGroup.alpha, hiddenOffset, 0f, true, animDuration));
    }

    private IEnumerator Animate(Vector2 fromPos, float fromAlpha, Vector2 toPos, float toAlpha, bool destroyOnEnd, float animDuration)
    {
        float elapsed = 0f;

        while (elapsed < animDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / animDuration);

            rect.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            yield return null;
        }

        rect.anchoredPosition = toPos;
        canvasGroup.alpha = toAlpha;

        if (destroyOnEnd && toAlpha == 0f)
            Destroy(gameObject);
    }
}
