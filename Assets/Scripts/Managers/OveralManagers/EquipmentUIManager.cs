using UnityEngine;
using UnityEngine.UI;

public class EquipmentUIManager : MonoBehaviour
{
    public Transform inventoryContainer; // Grid voor alle items die je hebt
    public Transform equippedContainer;  // Plek waar je huidige uitrusting staat
    public GameObject itemSlotPrefab;

    public void RefreshEquipmentUI()
    {
        var data = FindFirstObjectByType<TeamLoader>().data;

        // 1. Clear containers
        foreach (Transform child in inventoryContainer) Destroy(child.gameObject);
        foreach (Transform child in equippedContainer) Destroy(child.gameObject);

        // 2. Laat alle items in inventory zien
        foreach (var item in data.inventory)
        {
            GameObject slot = Instantiate(itemSlotPrefab, inventoryContainer);
            // Setup je UI (Icoon, Naam, Rarity kleur)
            slot.GetComponent<Button>().onClick.AddListener(() => {
                EquipmentManager.Instance.EquipItem(item.itemId);
                RefreshEquipmentUI();
            });
        }

        // 3. Laat uitgeruste items apart zien
        var equipped = EquipmentManager.Instance.GetEquippedItemsForCurrentDoobie();
        foreach (var item in equipped)
        {
            Instantiate(itemSlotPrefab, equippedContainer);
        }
    }
}