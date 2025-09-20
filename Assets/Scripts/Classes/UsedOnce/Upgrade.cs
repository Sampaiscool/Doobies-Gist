using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Upgrade
{
    public string upgradeName;
    public string description;
    public int cost;
    public Sprite icon;
    public UpgradeNames type;
    public CharacterPool Pool;
    public int intensity;
    public bool isCurse = false;

    public Upgrade(string name, string desc, int cost, UpgradeNames type, CharacterPool pool, int intensity, bool isCurse)
    {
        this.upgradeName = name;
        this.description = desc;
        this.cost = cost;
        this.type = type;
        this.Pool = pool;
        this.intensity = intensity;
        this.isCurse = isCurse;
    }
}

public enum UpgradeNames
{
    WeaponMastery,                        //None
    SpellSlinger,                         //None
    Firebrand,                            //None

    FleetingPetals,                       //Hiroshi
    UltimateBloom,                        //Hiroshi
    Deflector,                            //Hiroshi

    Sneaky,                               //None
    BloodyWeapon,                         //None

    ArcaneMind,                           //Menta

    ViolentAttacks,                       //None

    OffensiveFlow,                        //Hiroshi
    WhiteFlower,                          //Hiroshi
    StayPrepared,                         //Hiroshi

    HealtySupplies,                       //None

    GremlinHunger,                        //FatGremlin

    PhanthomTouch,                        //Cultist Member

    CriticalMonster,                      //None
    FlowersOfRot,                         //Phrox
    TargetFound,                          //None

    FleetingLife,                         //None - Curse
    SpellSorcerer,                        //None
    PowerSpells,                          //None
    FireFlies,                            //None

    VineLash,                             //Phrox
    TargetGarden,                         //Phrox
    FeelingGreen,                         //Phrox

    Careless,                             //None

    CriticalBarrels,                      //Cobb Silver Eye
    PaddedBarrels,                        //Cobb Silver Eye
    FiercePowder,                         //Cobb Silver Eye
    FlamingRum,                           //Cobb Silver Eye
    CriticalRum,                          //Cobb Silver Eye
    WalkThePlank,                         //Cobb Silver Eye
}
