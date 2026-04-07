using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerData
{
    public string selectedDoobieName = "";
    [FormerlySerializedAs("doobieTitle")] public Titles title;

    [Header("Global Stats")]
    public int savedXP;
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
    public string ravinName;
    public int harvestAmount; // The amount of vangurr that need to be defeated to harvest this Ravin tree
    // The thing this tree provides
}