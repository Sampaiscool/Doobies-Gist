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

    public override List<SkillSO> GetAllSkills()
    {
        if (_so.characterPool == CharacterPool.Zelstine)
        {
            if (CurrentGoddess != GoddessType.None && goddessSkills.ContainsKey(CurrentGoddess))
            {
                return goddessSkills[CurrentGoddess];
            }
            return _so.baseSkills;
        }

        return new List<SkillSO>(_so.baseSkills);
    }

    //  --- Zelstine ---
    private Dictionary<GoddessType, List<SkillSO>> goddessSkills = new();
    public GoddessType CurrentGoddess { get; private set; } = GoddessType.None;


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
            case ResourceType.Faith:
                var faith = new FaithResource(_so.baseResourceMax);
                faith.OnFaithGained += HandleFaithGained;
                MainResource = faith;
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

        if (_so.characterPool == CharacterPool.Zelstine)
        {
            goddessSkills[GoddessType.Kaelyth] = new List<SkillSO>(_so.skillSet1);
            goddessSkills[GoddessType.Velithra] = new List<SkillSO>(_so.skillSet2);
            goddessSkills[GoddessType.Elenara] = new List<SkillSO>(_so.skillSet3);
        }
    }

    /// <summary>
    /// Events that happen when you gain Rum
    /// </summary>
    /// <param name="amount">The amount you gain</param>
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

        int rumGained = Random.Range(1, 6);
        MainResource.Gain(rumGained);

        BattleUIManager.Instance.AddLog($"{CharacterName} Has made {rumGained} rum!");

        if (MainResource.Current >= 7)
        {
            if (MainResource.Current == MainResource.Max)
            {
                AddEffect(new Effect(EffectType.DefenceDown, 5, true, 10));
                AddEffect(new Effect(EffectType.WeaponStrenghten, 5, true, 10));
                AddEffect(new Effect(EffectType.SpellStrenghten, 5, true, 10));
                AddEffect(new Effect(EffectType.Regeneration, 5, true, 10));

                BattleUIManager.Instance.AddLog($"{CharacterName} Has entered a drunken brawl!");
            }
            else
            {
                AddEffect(new Effect(EffectType.DefenceDown, 5, true, 3));
                BattleUIManager.Instance.AddLog($"{CharacterName} Had a little to much to drink!");
            }
        }
        else
        {
            AddEffect(new Effect(EffectType.Harden, 5, true, 3));
        }
    }

    /// <summary>
    /// Events that happen when you gain Zurp
    /// </summary>
    /// <param name="amount">The amount you gain</param>
    private void HandleZurpGained(int amount)
    {

    }

    /// <summary>
    /// Events that happen when you gain Faith
    /// </summary>
    /// <param name="amount">The amount you gain</param>
    private void HandleFaithGained(int amount)
    {

    }

    public void SetGoddess(GoddessType goddess, bool applyDebuff = false)
    {
        if (_so.characterPool != CharacterPool.Zelstine)
            return; // Other doobies can’t do this

        if (applyDebuff && CurrentGoddess != GoddessType.None && goddess != CurrentGoddess)
        {
            //AddEffect(new Effect(EffectType.WeakenedFaith, 3, false, 1));
        }

        CurrentGoddess = goddess;
        BattleUIManager.Instance.AddLog($"{CharacterName} now worships {goddess}!");
    }
}
