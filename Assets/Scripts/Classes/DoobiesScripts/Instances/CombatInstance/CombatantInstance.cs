using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all combatants in the game
/// </summary>
public abstract class CombatantInstance
{
    public Transform animationAnchor;
    // Abstract properties
    public abstract ScriptableObject so { get; }
    public abstract string CharacterName { get; }
    public abstract Sprite CurrentImage { get; set; }
    public abstract int CurrentHealth { get; set; }
    public abstract int MaxHealth { get; set; }
    public abstract float CurrentDefence { get; set; }
    public abstract float CurrentSkillDmg { get; set; }
    public abstract float CurrentHealPower { get; set; }
    public abstract int CurrentBurnLevel { get; set; }
    public abstract int CurrentBurnDamage { get; set; }
    public abstract Transformations CurrentTransformation { get; set; }
    public abstract List<SkillSO> GetAllSkills();

    // Equipment and collections
    public WeaponInstance EquippedWeaponInstance;
    public List<Effect> ActiveEffects { get; private set; } = new List<Effect>();
    public List<GameObject> ActiveEffectIcons = new List<GameObject>();
    public List<Upgrade> ActiveUpgrades { get; private set; } = new List<Upgrade>();
    public List<Item> ActiveItems { get; private set; } = new List<Item>();

    // Controllers - Lazy initialization
    private DamageController _damageController;
    private HealingController _healingController;
    private EffectController _effectController;
    private UpgradeController _upgradeController;
    private CombatController _combatController;
    private TransformationController _transformationController;
    private AnimationController _animationController;
    private DefenseController _defenseController;

    protected DamageController DamageCtrl => _damageController ??= new DamageController(this);
    protected HealingController HealingCtrl => _healingController ??= new HealingController(this);
    protected EffectController EffectCtrl => _effectController ??= new EffectController(this);
    protected UpgradeController UpgradeCtrl => _upgradeController ??= new UpgradeController(this);
    protected CombatController CombatCtrl => _combatController ??= new CombatController(this);
    protected TransformationController TransformCtrl => _transformationController ??= new TransformationController(this);
    protected AnimationController AnimationCtrl => _animationController ??= new AnimationController(this);
    protected DefenseController DefenseCtrl => _defenseController ??= new DefenseController(this);

    // Weapon properties
    public float GetEffectiveWeaponDamage() => EquippedWeaponInstance?.GetEffectiveDamage() ?? 0;
    public int GetEffectiveCritChance() => EquippedWeaponInstance?.GetEffectiveCritChance() ?? 0;

    // Public API methods delegate to controllers
    public int HealCombatant(int amount) => HealingCtrl.HealCombatant(amount);

    public virtual (DamageResult result, int actualDamage) TakeDamage(int amount, bool isSkill, bool isEffect = false, bool ignoreDefense = false, SkillSO skill = null)
        => DamageCtrl.TakeDamage(amount, isSkill, isEffect, ignoreDefense, skill);

    public virtual string PerformBasicAttack(CombatantInstance target)
        => CombatCtrl.PerformBasicAttack(target);

    public float ApplyCriticalHitEffects(float baseDamage)
        => CombatCtrl.ApplyCriticalHitEffects(baseDamage);

    public float GetEffectiveWeaponDamageAfterEffects(float baseDamage)
        => DamageCtrl.GetEffectiveWeaponDamageAfterEffects(baseDamage);

    public float GetEffectiveWeaponDamageAfterEffectsForUI(float baseDamage)
        => DamageCtrl.GetEffectiveWeaponDamageAfterEffectsForUI(baseDamage);

    public float GetEffectiveSkillDamageAfterEffects(float baseDamage)
        => DamageCtrl.GetEffectiveSkillDamageAfterEffects(baseDamage);

    public int GetEffectiveCritChanceAfterEffects(int baseCrit)
        => CombatCtrl.GetEffectiveCritChanceAfterEffects(baseCrit);

    public float GetEffectiveSkillDamage(float baseDmg)
        => DamageCtrl.GetEffectiveSkillDamage(baseDmg);

    public float GetEffectiveSkillDamageForUI(float baseDmg)
        => DamageCtrl.GetEffectiveSkillDamageForUI(baseDmg);

    public float GetEffectiveHealPower(float baseHeal)
        => HealingCtrl.GetEffectiveHealPower(baseHeal);

    public float GetEffectiveDefence()
        => DefenseCtrl.GetEffectiveDefence();

    public void AddEffect(Effect newEffect, CombatantInstance source = null, int? sourceBurnLevel = null)
    {
        if (source != null)
            newEffect.sourceCombatant = source;
        else if (newEffect.type == EffectType.Burn)
        {
            // For burn effects in 1v1, default source is the opponent (who's attacking)
            newEffect.sourceCombatant = GetOpponent();
        }
        if (sourceBurnLevel.HasValue)
            newEffect.sourceBurnLevel = sourceBurnLevel;
        EffectCtrl.AddEffect(newEffect);
    }

    public void AddUpgrade(Upgrade newUpgrade)
        => UpgradeCtrl.AddUpgrade(newUpgrade);

    public void AddItem(Item newItem)
        => UpgradeCtrl.AddItem(newItem);

    public void TickEffects()
        => EffectCtrl.TickEffects();

    public void SetTransformation(Transformations transformation)
        => TransformCtrl.SetTransformation(transformation);

    public void PlayAttackAnimation(GameObject animationPrefab)
        => AnimationCtrl.PlayAttackAnimation(animationPrefab);

    public void PlayHitAnimation(GameObject animationPrefab)
        => AnimationCtrl.PlayHitAnimation(animationPrefab);

    public void ExplodeBarrels(CombatantInstance owner, CombatantInstance opponent, bool ownerIsBeingAttacked, bool cameFromPistolShot)
        => CombatCtrl.ExplodeBarrels(owner, opponent, ownerIsBeingAttacked, cameFromPistolShot);

    public void OnBurnDamage(int damage)
        => EffectCtrl.OnBurnDamage(damage);

    public void KillInstance() => CurrentHealth = 0;
    public CombatantInstance Opponent => GetOpponent();


    public CombatantInstance GetOpponent()
    {
        if (this is DoobieInstance)
            return GameManager.Instance.currentVangurr;
        else
            return GameManager.Instance.currentDoobie;
    }

    // Helper methods for easier lookups
    public Upgrade GetUpgrade(UpgradeNames type) => ActiveUpgrades.Find(u => u.type == type);
    public bool HasUpgrade(UpgradeNames type) => ActiveUpgrades.Exists(u => u.type == type);
    public int GetUpgradeStacks(UpgradeNames type) => ActiveUpgrades.FindAll(u => u.type == type).Count;
    public int GetUpgradeIntensity(UpgradeNames type) => GetUpgrade(type)?.intensity ?? 0;

    public Effect GetEffect(EffectType type) => ActiveEffects.Find(e => e.type == type);
    public bool HasEffect(EffectType type) => ActiveEffects.Exists(e => e.type == type);
    public int GetEffectIntensity(EffectType type) => GetEffect(type)?.intensity ?? 0;

    // Returns the total intensity for all effects whose enum name starts with the provided prefix
    public int GetTotalEffectIntensityByPrefix(string prefix)
    {
        int sum = 0;
        foreach (var e in ActiveEffects)
        {
            if (e.type.ToString().StartsWith(prefix))
                sum += e.intensity;
        }
        return sum;
    }

    // Returns true if any active effect's enum name starts with the provided prefix
    public bool HasEffectByPrefix(string prefix)
    {
        return ActiveEffects.Exists(e => e.type.ToString().StartsWith(prefix));
    }

    // Typed helpers using EffectGroup
    public int GetTotalEffectIntensity(EffectGroup group)
    {
        int sum = 0;
        foreach (var e in ActiveEffects)
        {
            if (e.type.ToGroup() == group)
                sum += e.intensity;
        }
        return sum;
    }

    public bool HasEffect(EffectGroup group)
    {
        return ActiveEffects.Exists(e => e.type.ToGroup() == group);
    }

    public Item GetItem(ItemType type) => ActiveItems.Find(i => i.type == type);
    public bool HasItem(ItemType type) => ActiveItems.Exists(i => i.type == type);

    // Convenience methods for common patterns
    public bool TryEffect(EffectType type, System.Action<Effect> action)
    {
        var eff = GetEffect(type);
        if (eff == null) return false;
        action(eff);
        return true;
    }

    public void RemoveEffect(EffectType type)
    {
        ActiveEffects.RemoveAll(e => e.type == type);
    }

    public void AddEffectStacks(EffectType type, int duration, bool isDebuff, int stacks)
    {
        for (int i = 0; i < stacks; i++)
            AddEffect(new Effect(type, duration, isDebuff, 1));
    }

    public bool WithUpgrade(UpgradeNames type, System.Action<Upgrade> action)
    {
        var upg = GetUpgrade(type);
        if (upg == null) return false;
        action(upg);
        return true;
    }

    public void DamageOpponentPerStack(UpgradeNames upgrade, int damagePerStack)
    {
        WithUpgrade(upgrade, upg =>
        {
            GetOpponent().TakeDamage(upg.intensity * damagePerStack, true);
        });
    }


    // Public methods for external access (used by skills)
    public void CheckForSkillOnUseEffects() => EffectCtrl.CheckForSkillOnUseEffects();

    // Internal methods for controllers to access
    internal void CheckForOnHealEffects() => EffectCtrl.CheckForOnHealEffects();
    internal void CheckForOverHealEffects() => EffectCtrl.CheckForOverHealEffects();
    internal void CheckForSpelllOnUseEffects() => EffectCtrl.CheckForSpelllOnUseEffects();
    internal void CheckForWeaponOnUseEffects() => EffectCtrl.CheckForWeaponOnUseEffects();
    internal void CheckForAttackEffects() => EffectCtrl.CheckForAttackEffects();
}
