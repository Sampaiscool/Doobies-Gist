using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DoobieInstance : CombatantInstance
{
    public DoobieSO _so;
    public override ScriptableObject so => _so;
    public override string CharacterName => _so.doobieName;
    public override Sprite CurrentImage { get; set; }

    public override int CurrentHealth { get; set; }
    public override int MaxHealth { get; set; }
    public override float CurrentDefence { get; set; }

    public override float CurrentSkillDmg { get; set; }
    public override float CurrentHealPower { get; set; }
    private int _currentBurnLevel = 1;
    private int _currentBurnDamage = 1;
    public override int CurrentBurnLevel { get => _currentBurnLevel; set => _currentBurnLevel = value; }
    public override int CurrentBurnDamage { get => _currentBurnDamage; set => _currentBurnDamage = value; }
    public override Transformations CurrentTransformation { get; set; }

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
        else if (_so.characterPool == CharacterPool.Thenghshou)
        {
            if (CurrentTransformation != Transformations.None && transformationSkills.ContainsKey(CurrentTransformation))
            {
                return transformationSkills[CurrentTransformation];
            }
            return _so.baseSkills;
        }

        return new List<SkillSO>(_so.baseSkills);
    }



    //  --- Zelstine ---
    private Dictionary<GoddessType, List<SkillSO>> goddessSkills = new();
    public GoddessType CurrentGoddess { get; private set; } = GoddessType.None;

    private Dictionary<Transformations, List<SkillSO>> transformationSkills = new();


    public DoobieInstance(DoobieSO so)
    {
        _so = so;
        CurrentImage = so.portrait;
        MaxHealth = so.baseHealth;
        CurrentHealth = MaxHealth;

        CurrentDefence = _so.baseDefence;

        CurrentSkillDmg = _so.skillDmg;
        CurrentHealPower = _so.healPower;

        CurrentBurnLevel = _so.baseBurnLevel;
        CurrentBurnDamage =  _so.baseBurnDamage;

        EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);

        CurrentTransformation = _so.startingTransformation;

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
            case ResourceType.Soulflow:
                var soulflow = new SoulflowResource(_so.baseResourceMax);
                soulflow.OnSoulflowGained += HandleSoulflowGained;
                soulflow.WorldEnergy.OnWorldEnergyGained += HandleWorldEnergyGained;
                soulflow.SpiritEnergy.OnSpiritEnergyGained += HandleSpiritEnergyGained;
                MainResource = soulflow;
                break;
            case ResourceType.Crystals:
                var crystals = new CrystalResource(_so.baseResourceMax);
                crystals.OnCrystalGained += HandleCrystalGained;
                MainResource = crystals;
                break;
            case ResourceType.Ember:
                var ember = new EmberResource(_so.baseResourceMax);
                ember.OnEmberGained += HandleEmberGained;
                MainResource = ember;
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
        foreach (var item in _so.startingItems)
        {
            if (item == null) continue;

            AddItem(new Item(
                item.itemName,
                item.description,
                item.cost,
                item.type,
                item.Pool,
                item.hasBeenPurchased
            )
            {
                icon = item.icon
            });
        }
        switch (_so.characterPool)
        {
            case CharacterPool.Zelstine:
                goddessSkills[GoddessType.Elenara] = new List<SkillSO>(_so.skillSet1);
                goddessSkills[GoddessType.Velithra] = new List<SkillSO>(_so.skillSet2);
                goddessSkills[GoddessType.Kaelyth] = new List<SkillSO>(_so.skillSet3);
                break;
            case CharacterPool.Thenghshou:
                transformationSkills[Transformations.WorldForm] = new List<SkillSO>(_so.skillSet1);
                transformationSkills[Transformations.SpiritForm] = new List<SkillSO>(_so.skillSet2);
                break;
            default:
                break;
        }
    }
    public void CheckForActionButtonClicked()
    {
        foreach (Upgrade upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.StoneHide:
                    AddEffect(new Effect(EffectType.Harden, 3, false, upgrade.intensity));
                    break;
                default:
                    break;
            }
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

    /// <summary>
    /// Events that happen when you gain Soulflow
    /// </summary>
    /// <param name="amount">The amount you gain</param>
    private void HandleSoulflowGained(int amount)
    {

    }
    /// <summary>
    /// Events that happen when you gain WorldEnergy
    /// </summary>
    /// <param name="amount">The amount you gain</param>
    private void HandleWorldEnergyGained(int amount)
    {
        if(MainResource is SoulflowResource soulflow && soulflow.WorldEnergy.Current >= soulflow.WorldEnergy.Max)
        {
            SetTransformation(Transformations.WorldForm);
        }
    }
    /// <summary>
    /// Events that happen when you gain SpiritEnergy
    /// </summary>
    /// <param name="amount">The amount you gain</param>
    private void HandleSpiritEnergyGained(int amount)
    {
        if (MainResource is SoulflowResource soulflow && soulflow.SpiritEnergy.Current >= soulflow.SpiritEnergy.Max)
        {
            SetTransformation(Transformations.SpiritForm);
        }
    }

    /// <summary>
    /// Events that happen when you gain Crystals
    /// </summary>
    /// <param name="amount">The amount you gain</param>
    private void HandleCrystalGained(int amount)
    {

    }
    
    /// <summary>
    /// Events that happen when you gain Crystals
    /// </summary>
    /// <param name="amount">The amount you gain</param>
    private void HandleEmberGained(int amount)
    {
        if (MainResource.Current == MainResource.Max)
        {
            if (HasUpgrade(UpgradeNames.EmberOverflow))
            {
                int upgradeIntensity = GetUpgradeIntensity(UpgradeNames.EmberOverflow);
                
                for (int i = 0; i < upgradeIntensity; i++)
                {
                    MainResource.GainMax(1); 
                }

                if (MainResource is EmberResource ember)
                {
                    ember.Current5();
                }
            }
        }
    }

    /// <summary>
    /// Set the current Goddess
    /// </summary>
    /// <param name="goddess">The chosen goddess</param>
    /// <param name="applyDebuff">Wheter you should give the player the debuff</param>
    public void SetGoddess(GoddessType goddess, bool applyDebuff = false)
    {
        if (_so.characterPool != CharacterPool.Zelstine)
            return;


        if (GameManager.Instance.InCombat && CurrentGoddess != GoddessType.None && goddess != CurrentGoddess)
        {
            //AddEffect(new Effect(EffectType.Holy, 3, true, 1));
            AddEffect(new Effect(EffectType.DefenceDown, 3, true, 1));
            AddEffect(new Effect(EffectType.TargetLocked, 3, true, 1));

            BattleUIManager.Instance.UpdateUI();
        }

        CurrentGoddess = goddess;

        string message = $"{CharacterName} now worships {goddess}!";
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.AddLog(message);

            BattleUIManager.Instance.RefreshSkillButtons(GetAllSkills());
            BattleUIManager.Instance.BackFromSpells();
        }
        else
        {
            Debug.Log(message);
        }
    }
}
