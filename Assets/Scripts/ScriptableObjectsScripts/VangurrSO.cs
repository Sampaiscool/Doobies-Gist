using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "SO/Vangurr")]
public class VangurrSO : ScriptableObject, ICombatantSO
{
    public string vangurrName;
    public Sprite portrait;
    public int skillDmg;
    public float baseDefence;
    public int baseHealth;
    public string VangurrText;
    public int difficultyLevel;

    public CharacterPool characterPool;

    public List<SkillSO> baseSkills; // Skills the Vangurr always has

    public WeaponSO defaultWeapon; // The weapon this vangurr starts with

    Sprite ICombatantSO.portrait => portrait;
    int ICombatantSO.baseHealth => baseHealth;
    CharacterPool ICombatantSO.CharacterPool => characterPool;
}
