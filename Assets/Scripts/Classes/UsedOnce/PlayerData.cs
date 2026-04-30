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
}

[System.Serializable]
public class RavinTree
{
    public string treeId;
    public SeedType seedType;
    public int growthRequired; // Total harvest needed to complete
    public int growthCurrent;  // Current growth progress
    public bool isComplete;
    public bool isClaimed;

    public RavinTree(SeedType type)
    {
        treeId = System.Guid.NewGuid().ToString();
        seedType = type;
        growthRequired = 10;
        growthCurrent = 0;
        isComplete = false;
        isClaimed = false;
    }
}