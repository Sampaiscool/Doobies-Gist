using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;
    private TeamLoader loader;

    private void Awake()
    {
        Instance = this;
        loader = FindFirstObjectByType<TeamLoader>();
    }

    /// <summary>
    /// Rust een item uit op de huidige geselecteerde Doobie.
    /// </summary>
    public void EquipItem(string itemId)
    {
        var data = loader.data;
        string currentDoobie = data.selectedDoobieName; // We pakken de geselecteerde doobie
        
        var doobieProgress = data.doobieProgressList.Find(d => d.doobieName == currentDoobie);
        var itemToEquip = data.inventory.Find(i => i.itemId == itemId);

        if (doobieProgress == null || itemToEquip == null) return;

        // 1. Zoek of er al een item van hetzelfde type (bijv. Sword) aan staat
        // We doen dit door door de huidige equipped items te loopen en hun type te checken
        string duplicateId = "";
        foreach(var id in doobieProgress.equippedItemIds)
        {
            var equippedItem = data.inventory.Find(i => i.itemId == id);
            if (equippedItem != null && equippedItem.itemType == itemToEquip.itemType)
            {
                duplicateId = id;
                break;
            }
        }

        // 2. Verwijder de oude (un-equip)
        if (!string.IsNullOrEmpty(duplicateId))
            doobieProgress.equippedItemIds.Remove(duplicateId);

        // 3. Voeg nieuwe toe
        doobieProgress.equippedItemIds.Add(itemId);
        
        loader.SaveGame();
        Debug.Log($"{itemToEquip.itemName} uitgerust op {currentDoobie}!");
    }

    /// <summary>
    /// Haalt alle items op die de huidige doobie draagt
    /// </summary>
    public List<EquippableItem> GetEquippedItemsForCurrentDoobie()
    {
        var data = loader.data;
        var doobie = data.doobieProgressList.Find(d => d.doobieName == data.selectedDoobieName);
        if (doobie == null) return new List<EquippableItem>();

        return data.inventory.Where(item => doobie.equippedItemIds.Contains(item.itemId)).ToList();
    }
}