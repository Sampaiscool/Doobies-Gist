using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text upgradeNameText;
    public TMP_Text intensity;
    public Image upgradeImage;
    public Image backgroundImage;
    private Upgrade upgradeData;

    private bool isLocked = false;
    private bool isFrozen = false;


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
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ShopManager shop = FindObjectOfType<ShopManager>();
            shop.FreezeUpgrade(upgradeData);

            SetFrozenVisual(shop.FrozenUpgrade == upgradeData);
            SetLocked(shop.FrozenUpgrade == upgradeData); // lock if frozen
        }
    }
    public void SetFrozenVisual(bool frozen)
    {
        isFrozen = frozen;
        backgroundImage.color = frozen ? Color.cyan : Color.white;
    }
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        GetComponent<Button>().interactable = !locked; // disable normal buy
    }
}
