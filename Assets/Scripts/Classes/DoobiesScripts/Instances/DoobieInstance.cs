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
    public override int CurrentHealPower { get; set; }

    public IResource MainResource { get; private set; }

    public override List<SkillSO> GetAllSkills() => new List<SkillSO>(_so.baseSkills);

    public DoobieInstance(DoobieSO so)
    {
        _so = so;

        MaxHealth = so.baseHealth;
        CurrentHealth = MaxHealth;

        CurrentDefence = _so.baseDefence;

        CurrentSkillDmg = _so.skillDmg;
        CurrentHealPower = _so.healPower;

        EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);

        // Pick correct resource implementation
        switch (_so.doobieMainResource)
        {
            case ResourceType.Zurp:
                MainResource = new ZurpResource(_so.baseResourceMax);
                break;
            default:
                MainResource = null;
                break;
        }

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
}
