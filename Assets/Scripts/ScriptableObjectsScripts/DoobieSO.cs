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
    public int zurp; // mana 
    public int baseDamage;

    public int baseHealth;
    public bool hasHealth = true;

    public List<SkillSO> baseSkills; // Skills the Doobie always has

    public WeaponSO defaultWeapon; // The weapon this Doobie starts with

    Sprite ICombatantSO.portrait => portrait;
    int ICombatantSO.baseHealth => baseHealth;
}
