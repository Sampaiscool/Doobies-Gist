using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BattleOptionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    [Header("UI")]
    public Button button;
    public TMP_Text labelText;
    public GameObject tooltipPanel;       // Assign in prefab
    public TMP_Text tooltipText;          // Assign in prefab

    [Header("Animation")]
    public Vector2 hiddenOffset = Vector2.zero;  // Start position of tooltip
    public Vector2 shownOffset = new Vector2(0f, 40f); // End position of tooltip
    public float fadeDuration = 0.15f;

    private RectTransform tooltipRect;
    private CanvasGroup canvasGroup;
    private Coroutine animRoutine;

    private string description;

    public void Setup(string label, UnityAction callback, string actionDescription = "")
    {
        labelText.text = label;
        button.onClick.RemoveAllListeners();
        if (callback != null)
            button.onClick.AddListener(callback);

        description = actionDescription;

        // Tooltip setup
        if (tooltipPanel != null)
        {
            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null) canvasGroup = tooltipPanel.AddComponent<CanvasGroup>();

            tooltipPanel.SetActive(false);
            canvasGroup.alpha = 0f;
            tooltipRect.anchoredPosition = hiddenOffset;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPanel == null || string.IsNullOrEmpty(description)) return;

        tooltipText.text = description;
        tooltipPanel.SetActive(true);

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(FadeAndSlide(hiddenOffset, shownOffset, true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipPanel == null) return;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(FadeAndSlide(tooltipRect.anchoredPosition, hiddenOffset, false));
    }
    public void OnPointerDown(PointerEventData eventData)
    {

        if (tooltipPanel == null) return;

        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(FadeAndSlide(tooltipRect.anchoredPosition, hiddenOffset, false));
    }

    private IEnumerator FadeAndSlide(Vector2 from, Vector2 to, bool fadeIn)
    {
        float elapsed = 0f;
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;

        tooltipRect.anchoredPosition = from;
        canvasGroup.alpha = startAlpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            tooltipRect.anchoredPosition = Vector2.Lerp(from, to, t);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }

        tooltipRect.anchoredPosition = to;
        canvasGroup.alpha = endAlpha;

        if (!fadeIn)
            tooltipPanel.SetActive(false);
    }
}
