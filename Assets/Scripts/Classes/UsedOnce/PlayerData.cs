using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum SeedType
{
    Turip,
    Doobie,
    Player
}

[System.Serializable]
public class PlayerData
{
    public string selectedDoobieName = "";
    [FormerlySerializedAs("doobieTitle")] public Titles title;

    [Header("Global Stats")]
    public int savedXP;
    public int harvest;
    public int turipSeeds;
    public int doobieSeeds;
    public int playerSeeds;
    public List<RavinTree> ravinTreeList;

    // Global Player Stats
    public int playerXP = 0;
    public int playerMastery = 0;

    public List<EquippableItem> inventory = new List<EquippableItem>();
    
    public List<DoobieProgress> doobieProgressList = new List<DoobieProgress>();
}

[System.Serializable]
public class DoobieProgress
{
    public string doobieName;
    public int doobieXP;
    public int doobieMastery;
    public Titles currentTitle;
    public bool isUnlocked;
    
    public List<string> equippedItemIds = new List<string>();
}

[System.Serializable]
public class RavinTree
{
    public string treeId;
    public int growthRequired; 
    public int growthCurrent;  
    public bool isComplete;
    public bool isClaimed;
    
    // Tijd-gebaseerde groei
    public bool isTimeBased;
    public long finishTimestamp; // UNIX timestamp wanneer hij klaar is

    public RavinTree(bool timeBased)
    {
        treeId = System.Guid.NewGuid().ToString();
        isTimeBased = timeBased;
        isComplete = false;
        isClaimed = false;

        if (timeBased)
        {
            // Klaar over bijvoorbeeld 1 uur (3600 seconden)
            finishTimestamp = System.DateTimeOffset.Now.ToUnixTimeSeconds() + 3600;
        }
        else
        {
            growthRequired = 5; // 5 Bosses
            growthCurrent = 0;
        }
    }
}

[System.Serializable]
public class ItemModifier
{
    public string modifierName;
    public float statBonus; // Bijv. 1.2 voor 20% extra damage
}

[System.Serializable]
public class EquippableItem
{
    public string itemId;
    public string itemName;
    public ItemType itemType;
    public Rarity rarity;
    public ItemModifier modifier;

    public EquippableItem()
    {
        itemId = System.Guid.NewGuid().ToString();
    }
}