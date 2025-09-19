using UnityEngine;
using TMPro;
using System.Collections;

public class BattleButtonTooltip : MonoBehaviour
{
    public TMP_Text tooltipText;
    public float slideDuration = 0.2f;
    public Vector2 hiddenOffset = new Vector2(0, -0);
    public Vector2 shownOffset = new Vector2(0, 50f);

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Coroutine slideRoutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        rect.anchoredPosition = hiddenOffset;
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }

    public void Show(string text)
    {
        tooltipText.text = text;
        gameObject.SetActive(true);

        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideAndFade(hiddenOffset, shownOffset, 0f, 1f));
    }

    public void Hide()
    {
        if (slideRoutine != null) StopCoroutine(slideRoutine);
        slideRoutine = StartCoroutine(SlideAndFade(rect.anchoredPosition, hiddenOffset, canvasGroup.alpha, 0f, true));
    }

    private IEnumerator SlideAndFade(Vector2 fromPos, Vector2 toPos, float fromAlpha, float toAlpha, bool deactivateOnEnd = false)
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);

            rect.anchoredPosition = Vector2.Lerp(fromPos, toPos, t);
            canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, t);

            yield return null;
        }

        rect.anchoredPosition = toPos;
        canvasGroup.alpha = toAlpha;

        if (deactivateOnEnd)
            gameObject.SetActive(false);
    }
}
