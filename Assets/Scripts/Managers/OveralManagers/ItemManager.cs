using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;
    private TeamLoader loader;

    private void Awake()
    {
        Instance = this;
        loader = FindFirstObjectByType<TeamLoader>();
    }

    public EquippableItem GenerateRandomItem()
    {
        EquippableItem newItem = new EquippableItem();
        newItem.itemType = (ItemType)Random.Range(0, 5);
        newItem.rarity = CalculateRandomRarity();
        newItem.itemName = $"{newItem.rarity} {newItem.itemType}";
        
        // Voeg een simpele modifier toe
        newItem.modifier = new ItemModifier { 
            modifierName = "Power Boost", 
            statBonus = 1.0f + (0.1f * (int)newItem.rarity) 
        };

        return newItem;
    }

    private Rarity CalculateRandomRarity()
    {
        float rand = Random.value;
        if (rand < 0.05f) return Rarity.Legendary;
        if (rand < 0.15f) return Rarity.Epic;
        if (rand < 0.40f) return Rarity.Rare;
        return Rarity.Common;
    }

    public void AddToInventory(EquippableItem item)
    {
        loader.data.inventory.Add(item);
        loader.SaveGame();
    }

    public bool EquipItemToDoobie(string doobieName, string itemId)
    {
        var data = loader.data;
        var doobie = data.doobieProgressList.Find(d => d.doobieName == doobieName);
        var item = data.inventory.Find(i => i.itemId == itemId);

        if (doobie == null || item == null) return false;

        // Check of we al een item van dit type aan hebben (un-equip vorige)
        doobie.equippedItemIds.RemoveAll(id => {
            var existingItem = data.inventory.Find(i => i.itemId == id);
            return existingItem != null && existingItem.itemType == item.itemType;
        });

        doobie.equippedItemIds.Add(itemId);
        loader.SaveGame();
        return true;
    }
}