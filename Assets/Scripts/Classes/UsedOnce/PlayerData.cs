using System.Collections.Generic;
using UnityEngine.Serialization;

[System.Serializable]
public class PlayerData
{
    public string selectedDoobieName = "";
    [FormerlySerializedAs("doobieTitle")] public Titles title;

    public int savedXP = 0;
    
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