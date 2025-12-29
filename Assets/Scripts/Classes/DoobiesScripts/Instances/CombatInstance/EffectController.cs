using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Manages effect application, ticking, and effect-based triggers
/// </summary>
public class EffectController
{
    private readonly CombatantInstance _combatant;

    public EffectController(CombatantInstance combatant)
    {
        _combatant = combatant;
    }

    public void AddEffect(Effect newEffect)
    {
        BeforeEffectGain(newEffect);

        if (newEffect.type == EffectType.TargetLocked)
        {
            _combatant.ActiveEffects.Add(newEffect);
        }
        else
        {
            StackOrAddEffect(newEffect);
        }

        EffectUpgradeHandler.CheckEffectUpgrades(_combatant, newEffect);
        UpdateEffectsUI();
        LogEffectGain(newEffect);
    }

    private void StackOrAddEffect(Effect newEffect)
    {
        Effect existing = _combatant.ActiveEffects.Find(b => b.type == newEffect.type);

        if (existing != null)
        {
            StackEffect(existing, newEffect);
        }
        else
        {
            _combatant.ActiveEffects.Add(newEffect);
        }
    }

    private void StackEffect(Effect existing, Effect newEffect)
    {
        switch (newEffect.type)
        {
            case EffectType.Stun:
            case EffectType.TimedBomb:
                existing.intensity += newEffect.intensity;
                break;
            default:
                existing.duration += newEffect.duration;
                existing.intensity += newEffect.intensity;
                break;
        }

        if (existing.iconInstance != null)
            existing.iconInstance.PlayEffect();
    }

    private void UpdateEffectsUI()
    {
        Transform effectContainer = _combatant is DoobieInstance
            ? BattleUIManager.Instance.DoobieEffectsContainer
            : BattleUIManager.Instance.VangurrEffectsContainer;

        BattleUIManager.Instance.UpdateEffectsUI(_combatant, effectContainer);
    }

    private void LogEffectGain(Effect newEffect)
    {
        string effectName = newEffect.iconGO != null ? newEffect.iconGO.name : newEffect.type.ToString();
        BattleUIManager.Instance.AddLog($"{_combatant.CharacterName} gains {newEffect.intensity} \"{effectName}\"!");
    }

    public void BeforeEffectGain(Effect newEffect)
    {
        Effect holyEffect = _combatant.ActiveEffects.Find(h => h.type == EffectType.Holy);
        if (holyEffect != null)
        {
            int holyDamage = holyEffect.intensity;
            _combatant.TakeDamage(holyDamage, true, true);
            BattleUIManager.Instance.AddLog($"{_combatant.CharacterName} takes damage because they gained a debuff while they have Holy!");
        }
    }

    public void TickEffects()
    {
        for (int i = _combatant.ActiveEffects.Count - 1; i >= 0; i--)
        {
            _combatant.ActiveEffects[i].duration--;

            if (_combatant.ActiveEffects[i].duration <= 0)
            {
                var expiredEffect = _combatant.ActiveEffects[i];
                HandleExpiredEffect(expiredEffect);
                _combatant.ActiveEffects.RemoveAt(i);
            }
        }

        UpdateEffectsUI();
    }

    private void HandleExpiredEffect(Effect expired)
    {
        switch (expired.type)
        {
            case EffectType.TargetLocked:
                TargetLockedHandler.Activate(_combatant, expired);
                break;
            case EffectType.TimedBomb:
                TimedBombHandler.Activate(_combatant, expired);
                break;
            case EffectType.Crystalize:
                _combatant.AddEffect(new Effect(EffectType.Harden, 3, false, expired.intensity));
                break;
        }
    }

    public void CheckForSpelllOnUseEffects()
    {
        CombatantInstance opponent = _combatant.GetOpponent();

        foreach (var upgrade in _combatant.ActiveUpgrades)
        {
            SpellUpgradeHandler.HandleSpellUpgrade(_combatant, opponent, upgrade);
        }

        var effectsSnapshot = new List<Effect>(_combatant.ActiveEffects);
        foreach (Item item in _combatant.ActiveItems)
        {
            SpellItemHandler.HandleSpellItem(_combatant, item, effectsSnapshot);
        }
    }

    public void CheckForWeaponOnUseEffects()
    {
        WeaponUpgradeHandler.HandleWeaponUpgrades(_combatant);
        WeaponEffectHandler.HandleWeaponEffects(_combatant);
    }

    public void CheckForOnHealEffects()
    {
        HealUpgradeHandler.HandleHealUpgrades(_combatant);
        HealEffectHandler.HandleHealEffects(_combatant);
    }

    public void CheckForOverHealEffects()
    {
        OverhealUpgradeHandler.HandleOverhealUpgrades(_combatant);
        OverhealEffectHandler.HandleOverhealEffects(_combatant);
    }
    public void CheckForSkillOnUseEffects()
    {

    }

    public void CheckForAttackEffects()
    {
        AttackUpgradeHandler.HandleAttackUpgrades(_combatant);
    }

    public void OnBurnDamage(int damage)
    {
        CombatantInstance opponent = _combatant.GetOpponent();
        BurnDamageHandler.HandleBurnDamage(_combatant, opponent, damage);
    }
}