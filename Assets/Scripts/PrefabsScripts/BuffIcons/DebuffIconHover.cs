using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class DebuffIconHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Buff linkedBuff;
    public GameObject tooltipPrefab;
    private GameObject tooltipInstance;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipPrefab == null || linkedBuff == null) return;

        tooltipInstance = Instantiate(tooltipPrefab, transform);
        tooltipInstance.transform.localPosition = new Vector3(0, 125, 0);

        TMP_Text tooltipText = tooltipInstance.GetComponentInChildren<TMP_Text>();
        if (tooltipText != null)
            tooltipText.text = $"{linkedBuff.type}\nTurns left: {linkedBuff.duration}\nStacks: {linkedBuff.intensity}";

        tooltipInstance.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipInstance != null)
            Destroy(tooltipInstance);
    }
}
