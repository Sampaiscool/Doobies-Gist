using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StatsUpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text upgradeNameText;
    public Image upgradeImage;
    public TMP_Text intensity;

    private Upgrade upgradeData;

    public void Setup(Upgrade upgrade)
    {
        upgradeData = upgrade;
        upgradeNameText.text = upgrade.upgradeName;

        if (upgrade.icon != null)
            upgradeImage.sprite = upgrade.icon;

        intensity.text = upgrade.intensity.ToString();
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        StatsUpgradeDescriptionPanel statsUpgradeDescriptionPanel = FindObjectOfType<StatsUpgradeDescriptionPanel>();
        statsUpgradeDescriptionPanel.ShowDescription(upgradeData);
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        StatsUpgradeDescriptionPanel statsUpgradeDescriptionPanel = FindObjectOfType<StatsUpgradeDescriptionPanel>();
        statsUpgradeDescriptionPanel.HideDescription();
    }
}
