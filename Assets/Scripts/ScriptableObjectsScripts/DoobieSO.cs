using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "SO/Doobie")]
public class DoobieSO : ScriptableObject, ICombatantSO
{
    public string doobieName;
    public Sprite portrait;
    public bool unlockedByDefault;

    public ResourceType doobieMainResource;

    public int baseResourceMax;

    public int skillDmg;
    public int healPower;
    public float baseDefence;
    public CharacterPool characterPool;

    public int baseHealth;
    public bool hasHealth = true;

    public List<SkillSO> baseSkills;
    public WeaponSO defaultWeapon;
    public List<Upgrade> startingUpgrades = new List<Upgrade>();

    Sprite ICombatantSO.portrait => portrait;
    int ICombatantSO.baseHealth => baseHealth;
    CharacterPool ICombatantSO.CharacterPool => characterPool;
}
