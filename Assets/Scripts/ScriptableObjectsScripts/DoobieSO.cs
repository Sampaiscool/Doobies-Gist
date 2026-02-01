using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "SO/Doobie")]
public class DoobieSO : ScriptableObject, ICombatantSO
{
    [SerializeField]
    [Header("UI and flavour")]
    public string doobieName;
    [TextArea]
    public string description;
    public Sprite portrait;

    [SerializeField]
    [Header("Gameplay")]
    public bool unlockedByDefault;
    public ResourceType doobieMainResource;
    public ScriptableObject resourceActionSO;
    public ScriptableObject doobieActionSO;
    public int baseResourceMax;
    public float skillDmg;
    public float healPower;
    public float baseDefence;
    public int baseBurnLevel = 1;
    public int baseBurnDamage = 1;
    public CharacterPool characterPool;
    public int baseHealth;
    public bool hasHealth = true;
    public Transformations startingTransformation;
    public List<SkillSO> baseSkills;
    public WeaponSO defaultWeapon;
    public List<Upgrade> startingUpgrades = new List<Upgrade>();
    public List<Item> startingItems = new List<Item>();
    Sprite ICombatantSO.portrait => portrait;
    int ICombatantSO.baseHealth => baseHealth;
    CharacterPool ICombatantSO.CharacterPool => characterPool;
    public bool canMulticast;

    public List<SkillSO> skillSet1;
    public List<SkillSO> skillSet2;
    public List<SkillSO> skillSet3;
}
