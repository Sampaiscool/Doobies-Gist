using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EffectEntryUI : MonoBehaviour
{
    [Header("UI References")]
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descText;

    /// <summary>
    /// Populates the entry with the given effect data.
    /// </summary>
    public void SetData(EffectDescriptionSO effect)
    {
        if (icon != null) icon.sprite = effect.Icon;
        if (nameText != null) nameText.text = effect.Name;
        if (descText != null) descText.text = effect.Description;
    }
}
