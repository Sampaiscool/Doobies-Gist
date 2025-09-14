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

    public void ShowDescription(Upgrade upgrade)
    {
        if (upgrade == null) return;

        descriptionText.text =
            $"{upgrade.description}\n" +
            $"Cost: {upgrade.cost}";
    }

    public void HideDescription()
    {
        descriptionText.text = "";
    }
}
