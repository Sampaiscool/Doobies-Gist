using System.Collections.Generic;
using UnityEngine;

public class VangurrInstance : CombatantInstance
{
    public VangurrSO _so;
    public override ScriptableObject so => _so;
    public override string CharacterName => _so.vangurrName;
    public override Sprite CurrentImage { get; set; }
    public override int CurrentHealth { get; set; }
    public override int MaxHealth { get; set; }
    public override float CurrentDefence { get; set; }
    public override int CurrentSkillDmg { get; set; }
    public override int CurrentHealPower { get; set; }

    public override Transformations CurrentTransformation { get; set; }

    public override List<SkillSO> GetAllSkills()
    {
        if (_so.characterPool == CharacterPool.Biyumi)
        {
            if (CurrentTransformation != Transformations.None && transformationSkills.ContainsKey(CurrentTransformation))
            {
                return transformationSkills[CurrentTransformation];
            }
            return _so.baseSkills;
        }

        return new List<SkillSO>(_so.baseSkills);
    }

    private Dictionary<Transformations, List<SkillSO>> transformationSkills = new();

    public VangurrInstance(VangurrSO so)
    {
        _so = so;
        CurrentImage = _so.portrait;
        CurrentHealth = _so.baseHealth;
        MaxHealth = _so.baseHealth;
        CurrentDefence = _so.baseDefence;
        CurrentSkillDmg = _so.skillDmg;
        CurrentHealPower = _so.healPower;
        EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);
        CurrentTransformation = _so.startingTransformation;

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

        switch (_so.characterPool)
        {
            case CharacterPool.Biyumi:
                transformationSkills[Transformations.None] = new List<SkillSO>(_so.baseSkills);
                transformationSkills[Transformations.BiyumiForm] = new List<SkillSO>(_so.skillSet1);
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public string PerformTurn(CombatantInstance target)
    {
        // Grab all skills
        List<SkillSO> skills = GetAllSkills();
        SkillSO chosenSkill = null;

        if (skills.Count > 0)
        {
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
