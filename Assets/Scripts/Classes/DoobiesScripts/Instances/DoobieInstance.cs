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
            Debug.Log($"GetAllSkills called! CurrentGoddess = {CurrentGoddess}");

            if (CurrentGoddess != GoddessType.None && goddessSkills.ContainsKey(CurrentGoddess))
            {
                Debug.Log($"Returning skills for {CurrentGoddess}");
                return goddessSkills[CurrentGoddess];
            }
            Debug.Log("Returning base skills");
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
            goddessSkills[GoddessType.Elenara] = new List<SkillSO>(_so.skillSet1);
            goddessSkills[GoddessType.Velithra] = new List<SkillSO>(_so.skillSet2);
            goddessSkills[GoddessType.Kaelyth] = new List<SkillSO>(_so.skillSet3);
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
            return;


        if (GameManager.Instance.InCombat && CurrentGoddess != GoddessType.None && goddess != CurrentGoddess)
        {
            AddEffect(new Effect(EffectType.Holy, 3, true, 1));
            AddEffect(new Effect(EffectType.DefenceDown, 3, true, 1));
            AddEffect(new Effect(EffectType.TargetLocked, 3, true, 1));

            BattleUIManager.Instance.UpdateUI();
        }

        CurrentGoddess = goddess;

        string message = $"{CharacterName} now worships {goddess}!";
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.AddLog(message);

            // Refresh skill buttons if the panel is open
            BattleUIManager.Instance.RefreshSkillButtons(GetAllSkills());
            BattleUIManager.Instance.BackFromSpells();
        }
        else
        {
            Debug.Log(message);
        }
    }
}
