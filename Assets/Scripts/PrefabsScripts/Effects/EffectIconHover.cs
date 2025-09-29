using UnityEngine;
using UnityEngine.EventSystems;

public class EffectIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Effect linkedEffect;
    public GameObject tooltipPrefab;

    private TooltipController tooltip;

    [Header("Animation")]
    public float animDuration = 0.2f;
    public Vector2 shownOffset = new Vector2(0, 150);
    public Vector2 hiddenOffset = new Vector2(0, 40f);

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPrefab == null || linkedEffect == null) return;

        // Instantiate tooltip as child of this effect
        GameObject tooltipInstance = Instantiate(tooltipPrefab, transform);
        tooltip = tooltipInstance.AddComponent<TooltipController>();

        // Disable raycast so tooltip doesn't block pointer events
        CanvasGroup cg = tooltipInstance.GetComponent<CanvasGroup>();
        if (cg == null) cg = tooltipInstance.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        string text = $"{linkedEffect.type}\nTurns: {linkedEffect.duration}\nIntensity: {linkedEffect.intensity}";
        tooltip.Init(text, hiddenOffset, shownOffset, animDuration);

        // Start at hidden offset relative to icon
        RectTransform iconRect = GetComponent<RectTransform>();
        tooltipInstance.transform.localPosition = hiddenOffset;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.Hide(hiddenOffset, animDuration);
            tooltip = null;
        }
    }
}
