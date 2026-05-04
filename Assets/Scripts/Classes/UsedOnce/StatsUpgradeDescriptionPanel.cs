using TMPro;
using UnityEngine;

public class StatsUpgradeDescriptionPanel : MonoBehaviour
{

    [SerializeField] private TMP_Text descriptionText;

    public void ShowDescriptionUpgrade(Upgrade upgrade)
    {
        if (upgrade == null) return;

        descriptionText.text = upgrade.description;
    }

    public void HideDescription()
    {
        descriptionText.text = "";
    }
}
