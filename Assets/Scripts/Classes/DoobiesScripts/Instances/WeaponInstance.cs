using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInstance
{
    public WeaponSO baseSO;
    public int bonusDamage = 0;
    public int bonusCritChance = 0;

    public WeaponInstance(WeaponSO baseSO)
    {
        this.baseSO = baseSO;
    }

    public int GetEffectiveDamage() => baseSO.baseDamage + bonusDamage;
    public int GetEffectiveCritChance() => baseSO.baseCritChance + bonusCritChance;
    public float MissChance => baseSO.missChance;
    public WeaponAttackData BasicAttack => baseSO.basicAttack;
    public GameObject Animation => baseSO.animation;
}
