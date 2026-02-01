using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles all damage calculations and damage-related effects
/// </summary>
public class DamageController
{
    private readonly CombatantInstance _combatant;

    public DamageController(CombatantInstance combatant)
    {
        _combatant = combatant;
    }

    public (DamageResult result, int actualDamage) TakeDamage(int amount, bool isSkill, bool isEffect = false, bool ignoreDefense = false, ScriptableObject skill = null)
    {
        if (HandleHidden())
        {
            BattleUIManager.Instance.AddLog($"{_combatant.CharacterName} was hidden and avoided the damage!");
            return (DamageResult.Missed, 0);
        }

        if (HandleDeflection())
            return (DamageResult.Deflected, 0);

        if (!isSkill)
        {
            if (HasEvasionEffect())
            {
                HandleDodgeEffects();
                return (DamageResult.Dodged, 0);
            }

            if (HandleSneaky())
                return (DamageResult.Dodged, 0);
        }

        float defence = CalculateDefence(ignoreDefense);
        int reducedDamage = Mathf.CeilToInt(amount / defence);

        if (HandleShield(reducedDamage))
            return (DamageResult.Blocked, 0);

        ApplyDamageToHealth(reducedDamage, isSkill, skill);

        return (DamageResult.Hit, reducedDamage);
    }

    private float CalculateDefence(bool ignoreDefense)
    {
        var vanishedDefenceEffect = _combatant.ActiveEffects.FindAll(b => b.type == EffectType.VanishedDefense);

        if (ignoreDefense || vanishedDefenceEffect.Count != 0)
            return Mathf.Min(1, _combatant.GetEffectiveDefence());

        return _combatant.GetEffectiveDefence();
    }

    private void ApplyDamageToHealth(int reducedDamage, bool isSkill, ScriptableObject skill)
    {
        _combatant.CurrentHealth = Mathf.Max(_combatant.CurrentHealth - reducedDamage, 0);

        if (reducedDamage > 0)
        {
            HandleOnDamage(reducedDamage, isSkill, skill);
            SpawnDamageText(reducedDamage);
            PlayDamageAnimation();
        }
    }

    private void SpawnDamageText(int damage)
    {
        var hpTransform = _combatant is VangurrInstance
            ? BattleUIManager.Instance.VangurrHP.transform
            : BattleUIManager.Instance.DoobieHP.transform;

        BattleUIManager.Instance.SpawnFloatingText("-" + damage, Color.red, hpTransform, true);
    }

    private void PlayDamageAnimation()
    {
        GameObject hitAnimPrefab = GameManager.Instance.damageAnimationPrefab;
        if (hitAnimPrefab != null)
            _combatant.PlayHitAnimation(hitAnimPrefab);
    }

    private bool HandleHidden()
    {
        return _combatant.ActiveEffects.Find(b => b.type == EffectType.Hidden) != null;
    }

    private bool HandleDeflection()
    {
        var deflectEffects = _combatant.ActiveEffects.FindAll(b => b.type == EffectType.Deflecion);
        if (deflectEffects.Count == 0) return false;

        DeflectionHandler.Handle(_combatant, deflectEffects);
        return true;
    }

    private bool HandleSneaky()
    {
        int sneakyStacks = _combatant.ActiveUpgrades.FindAll(u => u.type == UpgradeNames.Sneaky).Count;
        if (sneakyStacks == 0) return false;

        float dodgeChance = sneakyStacks * 0.20f;

        if (Random.value < dodgeChance)
        {
            _combatant.AddEffect(new Effect(EffectType.Evasion, sneakyStacks, false, sneakyStacks));
            return true;
        }

        return false;
    }

    private void HandleDodgeEffects()
    {
        var evasionEffect = _combatant.ActiveEffects.Find(b => b.type == EffectType.Evasion);
        if (evasionEffect == null) return;

        int stacks = Mathf.Max(1, evasionEffect.intensity);
        _combatant.AddEffect(new Effect(EffectType.WeaponStrenghten, 2, false, stacks));
        _combatant.AddEffect(new Effect(EffectType.CriticalEye, 2, false, stacks));

        evasionEffect.duration--;
        if (evasionEffect.duration <= 0)
            _combatant.ActiveEffects.Remove(evasionEffect);
    }

    private bool HasEvasionEffect()
    {
        return _combatant.ActiveEffects.Exists(b => b.type == EffectType.Evasion);
    }

    private bool HandleShield(int damage)
    {
        return ShieldHandler.HandleShield(_combatant, damage);
    }

    private void HandleOnDamage(int damage, bool isSkill, ScriptableObject skill)
    {
        OnDamageHandler.Handle(_combatant, damage, isSkill, skill as SkillSO);
    }

    public int GetEffectiveWeaponDamageAfterEffects(int baseDamage)
    {
        int modifiedDamage = baseDamage;
        var effectsSnapshot = new List<Effect>(_combatant.ActiveEffects);

        foreach (var effect in effectsSnapshot)
        {
            if (effect.type == EffectType.Rage)
            {
                modifiedDamage *= effect.intensity;
                _combatant.ActiveEffects.Remove(effect);
            }

            modifiedDamage = ApplyDamageModifier(modifiedDamage, effect);
        }

        return Mathf.Max(modifiedDamage, 0);
    }

    public int GetEffectiveWeaponDamageAfterEffectsForUI(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var effect in _combatant.ActiveEffects)
        {
            modifiedDamage = ApplyDamageModifier(modifiedDamage, effect);
        }

        return Mathf.Max(modifiedDamage, 0);
    }

    private int ApplyDamageModifier(int damage, Effect effect)
    {
        switch (effect.type)
        {
            case EffectType.WeaponWeaken:
                for (int i = 0; i < effect.intensity; i++)
                    damage = Mathf.FloorToInt(damage * 0.8f);
                break;
            case EffectType.WeaponStrenghten:
                for (int i = 0; i < effect.intensity; i++)
                    damage = Mathf.CeilToInt(damage * 1.2f);
                break;
        }
        return damage;
    }

    public int GetEffectiveSkillDamageAfterEffects(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var effect in _combatant.ActiveEffects)
        {
            switch (effect.type)
            {
                case EffectType.SpellWeaken:
                    for (int i = 0; i < effect.intensity; i++)
                        modifiedDamage = Mathf.FloorToInt(modifiedDamage * 0.8f);
                    break;
                case EffectType.SpellStrenghten:
                    for (int i = 0; i < effect.intensity; i++)
                        modifiedDamage = Mathf.CeilToInt(modifiedDamage * 1.2f);
                    break;
            }
        }

        GetEffectiveSkillDamageAfterUpgrades(modifiedDamage);

        return Mathf.Max(modifiedDamage, 0);
    }

    public int GetEffectiveSkillDamageAfterUpgrades(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (Upgrade Upgrade in _combatant.ActiveUpgrades)
        {
            switch (Upgrade.type)
            {
                case UpgradeNames.Pyromanian:
                    int requiredBurn = 10 - (Upgrade.intensity * 2);
                    requiredBurn = Mathf.Max(requiredBurn, 0);

                    CombatantInstance opponent = _combatant.Opponent;
                    int opponentBurn = opponent.GetTotalEffectIntensityByPrefix("Burn");

                    if (opponentBurn >= requiredBurn)
                    {
                        for (int i = 0; i < Upgrade.intensity; i++)
                        {
                            modifiedDamage = Mathf.CeilToInt(modifiedDamage * 1.5f);
                        }
                    }
                break;
            }
        }
        return modifiedDamage;
    }

    public int GetEffectiveSkillDamage(int baseDmg)
    {
        _combatant.CheckForSpelllOnUseEffects();
        return GetEffectiveSkillDamageAfterEffects(baseDmg);
    }

    public int GetEffectiveSkillDamageForUI(int baseDmg)
    {
        return GetEffectiveSkillDamageAfterEffects(baseDmg);
    }
}
