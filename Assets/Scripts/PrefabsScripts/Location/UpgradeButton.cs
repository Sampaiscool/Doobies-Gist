using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text upgradeNameText;
    public Image upgradeImage;
    private Upgrade upgradeData;

    private System.Action<Upgrade> onClickAction;

    public void Setup(Upgrade upgrade, System.Action<Upgrade> onClick)
    {
        upgradeData = upgrade;
        upgradeNameText.text = upgrade.upgradeName;
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
