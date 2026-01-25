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
    private Item itemData;

    public void SetupUpgrade(Upgrade upgrade)
    {
        upgradeData = upgrade;
        upgradeNameText.text = upgrade.upgradeName;

        if (upgrade.icon != null)
            upgradeImage.sprite = upgrade.icon;

        intensity.text = upgrade.intensity.ToString();
    }
    public void SetupItem(Item item)
    {
        itemData = item;
        upgradeNameText.text = item.itemName;

        if (item.icon != null)
            upgradeImage.sprite = item.icon;

        intensity.text = "";
    }

    public void OnPointerEnter(UnityEngine.EventSystems.PointerEventData eventData)
    {
        StatsUpgradeDescriptionPanel statsUpgradeDescriptionPanel = FindFirstObjectByType<StatsUpgradeDescriptionPanel>();

        if (upgradeData != null)
        {
            statsUpgradeDescriptionPanel.ShowDescriptionUpgrade(upgradeData);
        }
        else if (itemData != null)
        {
            statsUpgradeDescriptionPanel.ShowDescriptionItem(itemData);
        }
    }

    public void OnPointerExit(UnityEngine.EventSystems.PointerEventData eventData)
    {
        StatsUpgradeDescriptionPanel statsUpgradeDescriptionPanel = FindFirstObjectByType<StatsUpgradeDescriptionPanel>();
        statsUpgradeDescriptionPanel.HideDescription();
    }
}
