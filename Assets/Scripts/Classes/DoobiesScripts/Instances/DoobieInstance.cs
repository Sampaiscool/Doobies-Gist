using System.Collections.Generic;
using UnityEngine;

public class DoobieInstance : CombatantInstance
{
    public DoobieSO _so;
    public override ScriptableObject so => _so;
    public override string CharacterName => _so.doobieName;
    public override int CurrentHealth { get; set; }
    public override int MaxHealth { get; set; }
    public override float CurrentDefence { get; set; }

    public override int CurrentSkillDmg { get; set; }
    public override int CurrentHealPower { get ; set ; }

    public int CurrentZurp { get; set; }
    public int MaxZurp { get; set; }

    public override List<SkillSO> GetAllSkills() => new List<SkillSO>(_so.baseSkills);

    public DoobieInstance(DoobieSO so)
    {
        _so = so;

        MaxHealth = so.baseHealth;
        CurrentHealth = MaxHealth;

        CurrentDefence = _so.baseDefence;

        MaxZurp = so.zurp;
        CurrentZurp = MaxZurp;

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
                upgrade.intensity
            )
            {
                icon = upgrade.icon
            });
        }
    }

    public void ChangeZurp(int amount, bool isGain)
    {
        if (isGain)
        {
            CurrentZurp = Mathf.Min(CurrentZurp + amount, MaxZurp);
            OnZurpGainEffects();
        }
        else
        {
            CurrentZurp = Mathf.Max(CurrentZurp - amount, 0);
        }
    }
    void OnZurpGainEffects()
    {

    }
}
