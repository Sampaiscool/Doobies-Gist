using System.Collections.Generic;
using UnityEngine;

public class VangurrInstance : CombatantInstance
{
    public VangurrSO _so;
    public override ScriptableObject so => _so;
    public override string CharacterName => _so.vangurrName;
    public override int CurrentHealth { get; set; }
    public override int MaxHealth { get; set; }
    public override float CurrentDefence { get; set; }
    public override int CurrentSkillDmg { get; set; }

    public override List<SkillSO> GetAllSkills() => new List<SkillSO>(_so.baseSkills);

    public VangurrInstance(VangurrSO so)
    {
        _so = so;
        CurrentHealth = _so.baseHealth;
        MaxHealth = _so.baseHealth;
        CurrentDefence = _so.baseDefence;
        CurrentSkillDmg = _so.skillDmg;
        EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);
    }
    public string PerformTurn(CombatantInstance target)
    {
        // Decide: basic attack or skill
        List<SkillSO> skills = GetAllSkills();

        SkillSO chosenSkill = null;

        if (skills.Count > 0)
        {
            // Simple AI: 50/50 chance to use a skill
            if (Random.value < 0.5f)
                chosenSkill = skills[Random.Range(0, skills.Count)];
        }

        if (chosenSkill != null)
        {
            return chosenSkill.UseSkill(this, target);
        }
        else
        {
            return PerformBasicAttack(target);
        }
    }
}
