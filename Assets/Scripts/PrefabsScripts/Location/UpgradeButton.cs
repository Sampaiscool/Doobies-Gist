using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text upgradeNameText;
    public TMP_Text intensity;
    public Image upgradeImage;
    private Upgrade upgradeData;

    private System.Action<Upgrade> onClickAction;

    public Upgrade UpgradeData => upgradeData;

    public void Setup(Upgrade upgrade, System.Action<Upgrade> onClick)
    {
        upgradeData = upgrade;
        upgradeNameText.text = upgrade.upgradeName;

        int stack = GameManager.Instance.currentDoobie.ActiveUpgrades
                .Find(u => u.type == upgrade.type)?.intensity ?? 0;

        intensity.text = stack.ToString();

        upgradeImage.sprite = upgrade.icon;
        onClickAction = onClick;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => onClickAction?.Invoke(upgradeData));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        UpgradeDescriptionPanel.Instance?.ShowDescription(upgradeData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpgradeDescriptionPanel.Instance?.HideDescription();
    }
}
