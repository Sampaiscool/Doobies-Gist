using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static UnityEditor.FilePathAttribute;

public class LocationButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text locationNameText;
    public Image locationImage;
    private LocationSO locationData;

    private System.Action<LocationSO> onClickAction;

    public void Setup(LocationSO location, System.Action<LocationSO> onClick)
    {
        locationData = location;
        locationNameText.text = location.locationName;
        locationImage.sprite = location.locationImage;
        onClickAction = onClick;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => onClickAction?.Invoke(locationData));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Check if Instance is null for safety
        if (LocationDescriptionPanel.Instance != null)
        {
            LocationDescriptionPanel.Instance.ShowDescription(locationData); // Update description
        }
        else
        {
            Debug.LogWarning("LocationDescriptionPanel.Instance is null");
        }
    }


    public void OnPointerExit(PointerEventData eventData)
    {
        LocationDescriptionPanel.Instance.HideDescription();
    }
}
