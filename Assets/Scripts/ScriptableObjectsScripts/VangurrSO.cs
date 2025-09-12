using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "SO/Vangurr")]
public class VangurrSO : ScriptableObject, ICombatantSO
{
    public string vangurrName;
    public Sprite portrait;
    public int baseDamage;
    public int baseHealth;
    public string VangurrText;
    public int difficultyLevel;

    public List<SkillSO> baseSkills; // Skills the Vangurr always has

    public WeaponSO defaultWeapon; // The weapon this vangurr starts with

    Sprite ICombatantSO.portrait => portrait;
    int ICombatantSO.baseHealth => baseHealth;
}
