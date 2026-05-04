using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles combat actions like basic attacks and barrels
/// </summary>
public class CombatController
{
    private readonly CombatantInstance _combatant;

    public CombatController(CombatantInstance combatant)
    {
        _combatant = combatant;
    }

    public string PerformBasicAttack(CombatantInstance target)
    {
        if (_combatant.EquippedWeaponInstance == null)
            return $"{_combatant.CharacterName} tries to attack, but is unarmed!";

        if (CheckMiss())
            return $"{_combatant.CharacterName} swings at {target.CharacterName}, but misses!";

        float damage = CalculateDamage(out bool isCrit);
        
        PlayAttackAnimation(target);
        var (result, actualDamage) = target.TakeDamage((int)damage, false);

        ApplyPostAttackEffects(target);

        return FormatAttackResult(target, result, actualDamage, isCrit);
    }

    private bool CheckMiss()
    {
        Effect blindEffect = _combatant.ActiveEffects.Find(b => b.type == EffectType.Blind);
        return Random.value < _combatant.EquippedWeaponInstance.MissChance || blindEffect != null;
    }

    private float CalculateDamage(out bool isCrit)
    {
        float attack = _combatant.EquippedWeaponInstance.GetEffectiveDamage();
        float multiplier = Random.Range(0.5f, 1.5f);
        float baseDamage = attack * multiplier;
        float damageAfterEffects = _combatant.GetEffectiveWeaponDamageAfterEffects(baseDamage);

        isCrit = Random.Range(0, 100) < GetEffectiveCritChanceAfterEffects(_combatant.GetEffectiveCritChance());

        if (isCrit)
        {
            damageAfterEffects = ApplyCriticalHitEffects(damageAfterEffects);
        }

        return damageAfterEffects;
    }

    private void PlayAttackAnimation(CombatantInstance target)
    {
        if (_combatant.EquippedWeaponInstance.Animation != null)
        {
            target.PlayAttackAnimation(_combatant.EquippedWeaponInstance.Animation);
        }
    }

    private void ApplyPostAttackEffects(CombatantInstance target)
    {
        ApplyEffectsOnBasicAttack(target);
        _combatant.CheckForWeaponOnUseEffects();
        _combatant.CheckForAttackEffects();
    }

    private string FormatAttackResult(CombatantInstance target, DamageResult result, int actualDamage, bool isCrit)
    {
        switch (result)
        {
            case DamageResult.Deflected:
                return $"{_combatant.CharacterName} strikes, but {target.CharacterName} deflects the blow with finesse!";
            case DamageResult.Hit:
                ExplodeBarrels(_combatant, target, false, false);
                ExplodeBarrels(target, _combatant, true, false);
                return isCrit
                    ? $"{_combatant.CharacterName} lands a CRITICAL HIT on {target.CharacterName} for {actualDamage} damage!"
                    : $"{_combatant.CharacterName} strikes {target.CharacterName} for {actualDamage} damage!";
            case DamageResult.Missed:
                return $"{_combatant.CharacterName}'s attack phases through thin air!";
            case DamageResult.Immune:
                return $"{target.CharacterName} is immune to the attack!";
            case DamageResult.Blocked:
                return $"{target.CharacterName} blocks the hit and takes no damage!";
            case DamageResult.Dodged:
                return $"{target.CharacterName} swiftly dodges the attack!";
            default:
                return $"{_combatant.CharacterName} attacks, but something strange happens...";
        }
    }

    public float ApplyCriticalHitEffects(float baseDamage)
    {
        float modifiedDamage = baseDamage;

        foreach (var upgrade in _combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.CriticalMonster:
                    _combatant.AddEffect(new Effect(EffectType.CriticalEye, 3, false, upgrade.intensity));
                    modifiedDamage += upgrade.intensity + 1;
                    break;
            }
        }

        modifiedDamage *= 2;
        return modifiedDamage;
    }

    public int GetEffectiveCritChanceAfterEffects(int baseCrit)
    {
        int modifiedCrit = baseCrit;
        foreach (var effect in _combatant.ActiveEffects)
        {
            switch (effect.type)
            {
                case EffectType.CriticalEye:
                    modifiedCrit += effect.intensity * 5;
                    break;
            }
        }
        return Mathf.Clamp(modifiedCrit, 0, 100);
    }

    private void ApplyEffectsOnBasicAttack(CombatantInstance target)
    {
        foreach (var upgrade in _combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.Firebrand:
                    target.AddEffect(new Effect(EffectType.Burn, 3, true, upgrade.intensity));
                    break;
                case UpgradeNames.WrathboundOath:
                    if (_combatant is DoobieInstance doobie && doobie.MainResource.Current >= 5)
                    {
                        _combatant.AddEffect(new Effect(EffectType.WeaponStrenghten, 3, false, upgrade.intensity));
                        _combatant.AddEffect(new Effect(EffectType.CriticalEye, 3, false, upgrade.intensity));
                    }
                    break;
            }
        }

        var effectsSnapshot = new List<Effect>(_combatant.ActiveEffects);
        foreach (var effect in effectsSnapshot)
        {
            switch (effect.type)
            {
                case EffectType.Crystalize:
                    _combatant.AddEffect(new Effect(EffectType.Harden, 3, false, effect.intensity));
                    break;
            }
        }
    }

    public void ExplodeBarrels(CombatantInstance owner, CombatantInstance opponent, bool ownerIsBeingAttacked, bool cameFromPistolShot)
    {
        List<Effect> effectsSnapshot = new List<Effect>(owner.ActiveEffects);

        foreach (Effect effect in effectsSnapshot)
        {
            if (effect.type == EffectType.Barrel)
            {
                ExplodeBarrel(owner, opponent, effect, ownerIsBeingAttacked, cameFromPistolShot);
            }
        }
    }

    private void ExplodeBarrel(CombatantInstance owner, CombatantInstance opponent, Effect barrelEffect, bool ownerIsBeingAttacked, bool cameFromPistolShot)
    {
        CombatantInstance victim = ownerIsBeingAttacked ? owner : opponent;
        CombatantInstance attacker = ownerIsBeingAttacked ? opponent : owner;

        bool isCrit = Random.Range(0, 101) < GetEffectiveCritChanceAfterEffects(_combatant.GetEffectiveCritChance());
        int damage = CalculateBarrelDamage(owner, attacker, barrelEffect, isCrit, ownerIsBeingAttacked, cameFromPistolShot);

        var (result, damageDone) = victim.TakeDamage(damage, false, true);

        LogBarrelExplosion(owner, opponent, damageDone, isCrit, ownerIsBeingAttacked);
        owner.ActiveEffects.Remove(barrelEffect);

        HandleDualBarrels(owner, barrelEffect, ownerIsBeingAttacked);
    }

    private int CalculateBarrelDamage(CombatantInstance owner, CombatantInstance attacker, Effect barrelEffect, bool isCrit, bool ownerIsBeingAttacked, bool cameFromPistolShot)
    {
        float damage = barrelEffect.intensity + attacker.GetEffectiveWeaponDamageAfterEffects(attacker.GetEffectiveWeaponDamage());

        if (!ownerIsBeingAttacked)
        {
            Upgrade fiercePowder = owner.ActiveUpgrades.Find(b => b.type == UpgradeNames.FiercePowder);
            if (fiercePowder != null) damage += fiercePowder.intensity;
        }
        else
        {
            Upgrade paddedBarrels = owner.ActiveUpgrades.Find(b => b.type == UpgradeNames.PaddedBarrels);
            if (paddedBarrels != null) damage -= paddedBarrels.intensity;
        }

        if (isCrit)
        {
            damage = ApplyCriticalHitEffects(damage);
            HandleCriticalBarrels(owner, ownerIsBeingAttacked);
        }

        if (cameFromPistolShot)
        {
            damage *= 2;
            if (!ownerIsBeingAttacked)
            {
                int sploont = 10 * barrelEffect.intensity;
                GameManager.Instance.ChangeSploont(sploont, true);
                BattleUIManager.Instance.AddLog($"{_combatant.CharacterName} Gains {sploont} sploont!");
            }
        }

        return (int)damage;
    }

    private void HandleCriticalBarrels(CombatantInstance owner, bool ownerIsBeingAttacked)
    {
        Upgrade criticalBarrelUpgrade = owner.ActiveUpgrades.Find(b => b.type == UpgradeNames.CriticalBarrels);
        if (criticalBarrelUpgrade != null && !ownerIsBeingAttacked)
        {
            owner.AddEffect(new Effect(EffectType.CriticalEye, 3, false, criticalBarrelUpgrade.intensity));
            owner.AddEffect(new Effect(EffectType.Barrel, 100, false, criticalBarrelUpgrade.intensity));
        }
    }

    private void LogBarrelExplosion(CombatantInstance owner, CombatantInstance opponent, int damageDone, bool isCrit, bool ownerIsBeingAttacked)
    {
        string critText = isCrit ? " CRITICAL" : "";
        if (ownerIsBeingAttacked)
        {
            BattleUIManager.Instance.AddLog($"{owner.CharacterName}'s barrels explode backfiring \n dealing {damageDone}{critText} damage to themselves!\n");
        }
        else
        {
            BattleUIManager.Instance.AddLog($"{owner.CharacterName}'s barrels explode \n blasting {opponent.CharacterName} for {damageDone}{critText} damage!");
        }
    }

    private void HandleDualBarrels(CombatantInstance owner, Effect barrelEffect, bool ownerIsBeingAttacked)
    {
        
    }
}
/// <summary>
/// Handles all healing operations and healing-related effects
/// </summary>
public class HealingController
{
    private readonly CombatantInstance _combatant;

    public HealingController(CombatantInstance combatant)
    {
        _combatant = combatant;
    }

    /// <summary>
    /// Heal the combatant and activate all on-heal effects
    /// </summary>
    /// <param name="amount">The base heal amount</param>
    /// <returns>The actual amount healed</returns>
    public int HealCombatant(int amount)
    {
        float effectiveHeal = GetEffectiveHealPower(amount);
        effectiveHeal += _combatant.CurrentHealPower;

        int healAmount = CalculateActualHeal(effectiveHeal);

        if (healAmount > 0)
        {
            ApplyHeal(healAmount);
            LogHeal(healAmount);
        }

        CheckForOverheal((int)effectiveHeal);

        return healAmount;
    }

    /// <summary>
    /// Calculate how much health can actually be restored
    /// </summary>
    private int CalculateActualHeal(float effectiveHeal)
    {
        return Mathf.Min(Mathf.RoundToInt(effectiveHeal), _combatant.MaxHealth - _combatant.CurrentHealth);
    }

    /// <summary>
    /// Apply healing to the combatant and trigger heal effects
    /// </summary>
    private void ApplyHeal(int healAmount)
    {
        _combatant.CurrentHealth += healAmount;
        _combatant.CheckForOnHealEffects();
        SpawnHealText(healAmount);
    }

    /// <summary>
    /// Display floating heal text on the UI
    /// </summary>
    private void SpawnHealText(int healAmount)
    {
        var hpTransform = _combatant is VangurrInstance
            ? BattleUIManager.Instance.VangurrHP.transform
            : BattleUIManager.Instance.DoobieHP.transform;

        BattleUIManager.Instance.SpawnFloatingText("+" + healAmount, Color.green, hpTransform, false);
    }

    /// <summary>
    /// Add heal message to combat log
    /// </summary>
    private void LogHeal(int healAmount)
    {
        BattleUIManager.Instance.AddLog($"{_combatant.CharacterName} heals for {healAmount}!");
    }

    /// <summary>
    /// Check if healing exceeded max health and trigger overheal effects
    /// </summary>
    private void CheckForOverheal(int effectiveHeal)
    {
        if ((effectiveHeal + _combatant.CurrentHealth) > _combatant.MaxHealth)
        {
            _combatant.CheckForOverHealEffects();
        }
    }

    /// <summary>
    /// Get effective heal power after applying all modifiers
    /// </summary>
    /// <param name="baseHeal">Base heal amount before modifiers</param>
    /// <returns>Modified heal amount</returns>
    public float GetEffectiveHealPower(float baseHeal)
    {
        float modifiedHeal = baseHeal;

        foreach (var effect in _combatant.ActiveEffects)
        {
            if (effect.type == EffectType.HealingWeaken)
            {
                for (int i = 0; i < effect.intensity; i++)
                    modifiedHeal *= 0.8f;
            }
            else if (effect.type == EffectType.HealingStrenghten)
            {
                for (int i = 0; i < effect.intensity; i++)
                    modifiedHeal *= 1.2f;
            }
        }

        return Mathf.Max(0f, modifiedHeal);
    }
}

/// <summary>
/// Handles transformation logic
/// </summary>
public class TransformationController
{
    private readonly CombatantInstance _combatant;

    public TransformationController(CombatantInstance combatant)
    {
        _combatant = combatant;
    }

    public void SetTransformation(Transformations transformation)
    {
        _combatant.CurrentTransformation = transformation;
        Debug.Log($"Current Transformation: {_combatant.CurrentTransformation} / Chosen: {transformation}");

        BattleUIManager.Instance.AddLog($"{_combatant.CharacterName} has transformed!");
        BattleUIManager.Instance.CombatantTransformation(_combatant, transformation);

        OnTransformation();

        BattleUIManager.Instance.RefreshSkillButtons(_combatant.GetAllSkills());
    }

    private void OnTransformation()
    {
        foreach (var upgrade in _combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.BloodiedMomentum:
                    CombatantInstance target = _combatant is DoobieInstance
                        ? GameManager.Instance.currentVangurr
                        : GameManager.Instance.currentDoobie;
                    target.AddEffect(new Effect(EffectType.Bleed, 3, true, upgrade.intensity * 3));
                    break;
            }
        }
    }
}

/// <summary>
/// Handles animation playback
/// </summary>
public class AnimationController
{
    private readonly CombatantInstance _combatant;

    public AnimationController(CombatantInstance combatant)
    {
        _combatant = combatant;
    }

    public void PlayAttackAnimation(GameObject animationPrefab)
    {
        if (animationPrefab == null || _combatant.animationAnchor == null)
            return;

        GameObject spawned = InstantiateAnimation(animationPrefab);
        SetupRenderers(spawned);
        SetAutoDestroy(spawned);
    }

    public void PlayHitAnimation(GameObject animationPrefab)
    {
        if (animationPrefab == null || _combatant.animationAnchor == null)
            return;

        GameObject spawned = GameObject.Instantiate(animationPrefab, _combatant.animationAnchor.position, Quaternion.identity);
        spawned.transform.SetParent(_combatant.animationAnchor);
        spawned.transform.localScale.Normalize();

        var ps = spawned.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var renderer = ps.GetComponent<Renderer>();
            renderer.sortingLayerName = "Foreground";
            renderer.sortingOrder = 20;
        }

        GameObject.Destroy(spawned, 2f);
    }

    private GameObject InstantiateAnimation(GameObject animationPrefab)
    {
        GameObject spawned = GameObject.Instantiate(
            animationPrefab,
            _combatant.animationAnchor.position,
            _combatant.animationAnchor.rotation,
            _combatant.animationAnchor
        );
        spawned.transform.localScale = animationPrefab.transform.localScale;
        spawned.SetActive(true);
        return spawned;
    }

    private void SetupRenderers(GameObject spawned)
    {
        foreach (var renderer in spawned.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sortingLayerName = "VFXForeground";
            renderer.sortingOrder = 100;
        }
    }

    private void SetAutoDestroy(GameObject spawned)
    {
        float lifeTime = 2f;
        var ps = spawned.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            lifeTime = ps.main.duration + ps.main.startLifetime.constantMax;
        }
        GameObject.Destroy(spawned, lifeTime);
    }
}

/// <summary>
/// Handles defense calculations
/// </summary>
public class DefenseController
{
    private readonly CombatantInstance _combatant;

    public DefenseController(CombatantInstance combatant)
    {
        _combatant = combatant;
    }

    public float GetEffectiveDefence()
    {
        float defence = _combatant.CurrentDefence;

        foreach (var effect in _combatant.ActiveEffects)
        {
            if (effect.type == EffectType.DefenceDown)
            {
                for (int i = 0; i < effect.intensity; i++)
                    defence *= 0.8f;
            }
            else if (effect.type == EffectType.Harden)
            {
                for (int i = 0; i < effect.intensity; i++)
                    defence *= 1.2f;
            }
        }

        switch (_combatant.CurrentTransformation)
        {
            case Transformations.SpiritForm:
                defence *= 0.5f;
                break;
        }

        return defence;
    }
}

/// <summary>
/// Handles upgrade and item management
/// </summary>
public class UpgradeController
{
    private readonly CombatantInstance _combatant;

    public UpgradeController(CombatantInstance combatant)
    {
        _combatant = combatant;
    }

    public void AddUpgrade(Upgrade newUpgrade)
    {
        Upgrade existing = _combatant.ActiveUpgrades.Find(b => b.type == newUpgrade.type);
        if (existing != null)
        {
            existing.intensity += newUpgrade.intensity;
        }
        else
        {
            _combatant.ActiveUpgrades.Add(newUpgrade);
        }
    }
}