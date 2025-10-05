using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeDescriptionPanel : MonoBehaviour
{
    public static UpgradeDescriptionPanel Instance { get; private set; }

    [SerializeField] private TMP_Text descriptionText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        descriptionText.text = "";
    }

    public void ShowDescriptionUpgrade(Upgrade upgrade)
    {
        if (upgrade == null) return;

        descriptionText.text =
            $"{upgrade.description}\n" +
            $"Cost: {upgrade.cost} Sploont";
    }
    public void ShowDescriptionItem(Item item)
    {
        if (item == null) return;

        descriptionText.text =
            $"{item.description}\n" +
            $"Cost: {item.cost} Dzeef";
    }

    public void HideDescription()
    {
        descriptionText.text = "";
    }
}
