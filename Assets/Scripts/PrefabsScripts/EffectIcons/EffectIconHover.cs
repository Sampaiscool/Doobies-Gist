using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class EffectIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Effect linkedEffect;
    public GameObject tooltipPrefab;
    private GameObject tooltipInstance;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPrefab == null || linkedEffect == null) return;

        tooltipInstance = Instantiate(tooltipPrefab, transform);
        tooltipInstance.transform.localPosition = new Vector3(0, 125, 0);

        TMP_Text tooltipText = tooltipInstance.GetComponentInChildren<TMP_Text>();
        if (tooltipText != null)
            tooltipText.text = $"{linkedEffect.type}\nTurns left: {linkedEffect.duration}\nIntensity: {linkedEffect.intensity}";

        tooltipInstance.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipInstance != null)
            Destroy(tooltipInstance);
    }
}
