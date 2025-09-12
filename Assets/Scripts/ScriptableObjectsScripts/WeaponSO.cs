using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapons/Weapon")]
public class WeaponSO : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    public GameObject animation;

    public int baseDamage;
    public int baseCritChance;

    

    [Range(0, 1f)] public float missChance = 0.1f;

    public WeaponAttackData basicAttack;
}

[System.Serializable]
public class WeaponAttackData
{
    public int damage;
    public DamageType type;
}
