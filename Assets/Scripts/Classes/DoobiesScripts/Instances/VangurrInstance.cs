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

    private Dictionary<Transformations, List<SkillSO>> transformationSkills = new();

    public VangurrInstance(VangurrSO so)
    {
        _so = so;
        // Do NOT touch Unity objects here (like Sprites, Weapons, etc.)
    }

    /// <summary>
    /// Call this after construction to safely initialize Unity objects and data
    /// </summary>
    public void Init()
    {
        // Basic stats
        CurrentImage = _so.portrait;
        CurrentHealth = _so.baseHealth;
        MaxHealth = _so.baseHealth;
        CurrentDefence = _so.baseDefence;
        CurrentSkillDmg = _so.skillDmg;
        CurrentHealPower = _so.healPower;

        // Weapon
        if (_so.defaultWeapon != null)
            EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);

        // Transformation
        CurrentTransformation = _so.startingTransformation;

        // Upgrades
        foreach (var upgrade in _so.startingUpgrades)
        {
            if (upgrade == null) continue;

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

        // Items
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

        // Transformation skills (Biyumi example)
        if (_so.characterPool == CharacterPool.Biyumi)
        {
            transformationSkills[Transformations.None] = new List<SkillSO>(_so.baseSkills ?? new List<SkillSO>());
            transformationSkills[Transformations.BiyumiForm] = new List<SkillSO>(_so.skillSet1 ?? new List<SkillSO>());
        }
    }

    public override List<SkillSO> GetAllSkills()
    {
        if (_so.characterPool == CharacterPool.Biyumi)
        {
            if (CurrentTransformation != Transformations.None && transformationSkills.ContainsKey(CurrentTransformation))
                return transformationSkills[CurrentTransformation];

            return _so.baseSkills;
        }

        return new List<SkillSO>(_so.baseSkills);
    }

    public string PerformTurn(CombatantInstance target)
    {
        List<SkillSO> skills = GetAllSkills();
        SkillSO chosenSkill = null;

        if (skills.Count > 0)
        {
            float chance = _so.skillChance / 100f;

            if (Random.value < chance)
                chosenSkill = skills[Random.Range(0, skills.Count)];
        }

        if (chosenSkill != null)
            return chosenSkill.UseSkill(this, target);

        return PerformBasicAttack(target);
    }
}
