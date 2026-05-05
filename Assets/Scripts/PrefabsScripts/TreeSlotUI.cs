using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class TreeTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltipVisual; // Het zwarte balkje/vlakje
    public TMP_Text tooltipText;     // De tekst in het balkje
    private string currentData;

    public void SetTooltipData(string data)
    {
        currentData = data;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (string.IsNullOrEmpty(currentData)) return;
        tooltipVisual.SetActive(true);
        tooltipText.text = currentData;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipVisual.SetActive(false);
    }
}