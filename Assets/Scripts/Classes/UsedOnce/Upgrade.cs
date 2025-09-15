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

    public Upgrade(string name, string desc, int cost, UpgradeNames type, CharacterPool pool, int intensity)
    {
        this.upgradeName = name;
        this.description = desc;
        this.cost = cost;
        this.type = type;
        this.Pool = pool;
        this.intensity = intensity;
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
}
