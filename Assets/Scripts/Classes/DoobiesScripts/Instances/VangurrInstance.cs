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
    public override int CurrentHealPower { get; set; }

    public override List<SkillSO> GetAllSkills() => new List<SkillSO>(_so.baseSkills);

    public VangurrInstance(VangurrSO so)
    {
        _so = so;
        CurrentHealth = _so.baseHealth;
        MaxHealth = _so.baseHealth;
        CurrentDefence = _so.baseDefence;
        CurrentSkillDmg = _so.skillDmg;
        CurrentHealPower = _so.healPower;
        EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);

        foreach (var upgrade in _so.startingUpgrades)
        {
            AddUpgrade(new Upgrade(
                upgrade.upgradeName,
                upgrade.description,
                upgrade.cost,
                upgrade.type,
                upgrade.Pool,
                upgrade.intensity,
                upgrade.isCurse
            )
            {
                icon = upgrade.icon
            });
        }
    }
    public string PerformTurn(CombatantInstance target)
    {
        // Grab all skills
        List<SkillSO> skills = GetAllSkills();
        SkillSO chosenSkill = null;

        if (skills.Count > 0)
        {
            // Skill chance is percentage (e.g. 30 = 30%)
            float chance = _so.skillChance / 100f;

            if (Random.value < chance)
            {
                chosenSkill = skills[Random.Range(0, skills.Count)];
            }
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
