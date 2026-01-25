using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponInstance
{
    /// <summary>
    /// So of the instance
    /// </summary>
    public WeaponSO baseSO;
    /// <summary>
    /// Damage that gets added to the so baseDamage
    /// </summary>
    public int bonusDamage = 0;
    /// <summary>
    /// Crit chance that gets added to the so baseDamage
    /// </summary>
    public int bonusCritChance = 0;

    public WeaponInstance(WeaponSO baseSO)
    {
        this.baseSO = baseSO;
    }

    /// <summary>
    /// Gets the weapon damage (base damage + bonus damage)
    /// </summary>
    /// <returns>The damage amount</returns>
    public int GetEffectiveDamage() => baseSO.baseDamage + bonusDamage;
    /// <summary>
    /// Gets the weapon crit chance (base crit + bonus crit)
    /// </summary>
    /// <returns>the crit chance (0-100)</returns>
    public int GetEffectiveCritChance() => baseSO.baseCritChance + bonusCritChance;
    /// <summary>
    /// Chance to miss (0 - 1)
    /// </summary>
    public float MissChance => baseSO.missChance;
    /// <summary>
    /// Damage and attack type
    /// </summary>
    public WeaponAttackData BasicAttack => baseSO.basicAttack;
    /// <summary>
    /// Animation that gets played when you basic attack
    /// </summary>
    public GameObject Animation => baseSO.animation;
}
