using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocationDescriptionPanel : MonoBehaviour
{
    public static LocationDescriptionPanel Instance;

    public GameObject panelRoot; // the entire panel
    public TMP_Text descriptionText;

    private void Awake()
    {
        // Ensure that Instance is set to this object
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates if the object is duplicated
        }
    }

    public void ShowDescription(LocationSO location)
    {
        descriptionText.text = location.description; // Update the description text
    }

    public void HideDescription()
    {
        descriptionText.text = "";
    }
}
