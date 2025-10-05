using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class UpgradeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text upgradeNameText;
    public TMP_Text intensity;
    public Image upgradeImage;
    public Image backgroundImage;
    private Upgrade upgradeData;

    private bool isLocked = false;
    private bool isFrozen = false;


    private System.Action<Upgrade> onClickActionUpgrade;
    private System.Action<Item> onClickActionItem;

    public Upgrade UpgradeData => upgradeData;
    public Item ItemData { get; private set; }

    public void Setup(Upgrade upgrade, System.Action<Upgrade> onClick)
    {
        upgradeData = upgrade;
        upgradeNameText.text = upgrade.upgradeName;

        int stack = GameManager.Instance.currentDoobie.ActiveUpgrades
                .Find(u => u.type == upgrade.type)?.intensity ?? 0;

        intensity.text = stack.ToString();

        upgradeImage.sprite = upgrade.icon;
        backgroundImage.color = Color.white;
        onClickActionUpgrade = onClick;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => onClickActionUpgrade?.Invoke(upgradeData));
    }
    public void SetupAsItem(Item item, System.Action<Item> onBuy)
    {
        ItemData = item;
        upgradeNameText.text = item.itemName;
        upgradeImage.sprite = item.icon;
        backgroundImage.color = Color.gray;
        intensity.text = "";
        onClickActionItem = onBuy;

        GetComponent<Button>().onClick.RemoveAllListeners();
        GetComponent<Button>().onClick.AddListener(() => onClickActionItem?.Invoke(ItemData));
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UpgradeData != null)
        {
            UpgradeDescriptionPanel.Instance?.ShowDescriptionUpgrade(upgradeData);
        }
        else
        {
            UpgradeDescriptionPanel.Instance?.ShowDescriptionItem(ItemData);
        }
            
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UpgradeDescriptionPanel.Instance?.HideDescription();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ShopManager shop = FindFirstObjectByType<ShopManager>();
            shop.FreezeUpgrade(upgradeData);

            SetFrozenVisual(GameManager.Instance.frozenUpgrade == upgradeData);
            SetLocked(GameManager.Instance.frozenUpgrade == upgradeData); // lock if frozen
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
