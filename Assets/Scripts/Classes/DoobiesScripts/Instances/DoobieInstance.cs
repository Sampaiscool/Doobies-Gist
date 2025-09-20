using System.Collections.Generic;
using Unity.VisualScripting;
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
                var zurp = new ZurpResource(_so.baseResourceMax);
                zurp.OnZurpGained += HandleZurpGained;
                MainResource = zurp;
                break;
            case ResourceType.Health:
                MainResource = new HealthResource(_so.baseResourceMax);
                break;
            case ResourceType.Rum:
                var rum = new RumResource(_so.baseResourceMax);
                rum.OnRumGained += HandleRumGained;
                MainResource = rum;
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
                upgrade.intensity,
                upgrade.isCurse
            )
            {
                icon = upgrade.icon
            });
        }
    }
    private void HandleRumGained(int amount)
    {
        Upgrade criticalRum = ActiveUpgrades.Find(r => r.type == UpgradeNames.CriticalRum);
        if (criticalRum != null)
        {
            AddEffect(new Effect(EffectType.CriticalEye, 2, false, criticalRum.intensity));
        }

        Upgrade flamingRum = ActiveUpgrades.Find(r => r.type == UpgradeNames.FlamingRum);
        if (flamingRum != null && GameManager.Instance.currentVangurr != null)
        {
            GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Burn, flamingRum.intensity, true, flamingRum.intensity));
        }
    }
    private void HandleZurpGained(int amount)
    {

    }
}
