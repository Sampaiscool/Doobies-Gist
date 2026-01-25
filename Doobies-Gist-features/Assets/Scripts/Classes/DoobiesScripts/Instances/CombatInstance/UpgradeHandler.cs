using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Handles upgrade triggers when effects are gained
/// </summary>
public static class EffectUpgradeHandler
{
    public static void CheckEffectUpgrades(CombatantInstance combatant, Effect newEffect)
    {
        CombatantInstance opponent = combatant.GetOpponent();

        if (newEffect.isDebuff)
        {
            HandleCursedFaith(combatant);
        }

        switch (newEffect.type)
        {
            case EffectType.Deflecion:
                HandleDeflectionUpgrades(combatant);
                break;
            case EffectType.SpellWeaken:
                HandlePowerSpells(combatant);
                break;
            case EffectType.Hidden:
                HandleHowlingRush(combatant);
                break;
            case EffectType.Bleed:
                HandleSoulflareEdge(opponent);
                break;
            case EffectType.WeaponStrenghten:
                HandleFuryStrike(combatant, opponent);
                break;
            case EffectType.Stun:
                HandleStunningStrike(combatant, opponent);
                break;
            case EffectType.Burn:
                HandleBurningHands(opponent);
                break;
        }

        HandleMaskOfMidnight(combatant);
    }

    private static void HandleCursedFaith(CombatantInstance combatant)
    {
        if (combatant is not DoobieInstance doobie || doobie.CurrentGoddess != GoddessType.Velithra)
            return;

        doobie.WithUpgrade(UpgradeNames.CursedFaith, upg =>
        {
            doobie.MainResource.Gain(upg.intensity);
            BattleUIManager.Instance.AddLog($"{combatant.CharacterName} has gained {upg.intensity} Faith!");
        });
    }

    private static void HandleDeflectionUpgrades(CombatantInstance c)
    {
        c.WithUpgrade(UpgradeNames.FleetingPetals, upg =>
            c.HealCombatant(upg.intensity));

        if (c is DoobieInstance d && d.MainResource?.Type == ResourceType.Zurp)
        {
            c.WithUpgrade(UpgradeNames.WhiteFlower, upg =>
                d.MainResource.Gain(upg.intensity));
        }
    }

    private static void HandlePowerSpells(CombatantInstance c)
    {
        c.WithUpgrade(UpgradeNames.PowerSpells, upg =>
        {
            c.TryEffect(EffectType.SpellWeaken, weaken =>
            {
                weaken.duration -= upg.intensity;
                weaken.intensity -= upg.intensity;
            });
        });
    }

    private static void HandleHowlingRush(CombatantInstance c)
    {
        c.WithUpgrade(UpgradeNames.HowlingRush, upg =>
            c.AddEffect(new Effect(
                EffectType.Regeneration,
                1,
                false,
                upg.intensity * 5
            )));
    }

    private static void HandleSoulflareEdge(CombatantInstance opponent)
    {
        opponent.WithUpgrade(UpgradeNames.SoulflareEdge, upg =>
            opponent.HealCombatant(upg.intensity));
    }

    private static void HandleFuryStrike(CombatantInstance owner, CombatantInstance opponent)
    {
        owner.WithUpgrade(UpgradeNames.FuryStrike, upg =>
            opponent.TakeDamage(upg.intensity, true));
    }

    private static void HandleStunningStrike(CombatantInstance combatant, CombatantInstance opponent)
    {
        opponent.WithUpgrade(UpgradeNames.StunningStrike, upg =>
            combatant.TakeDamage(upg.intensity, true));
    }

    private static void HandleBurningHands(CombatantInstance opponent)
    {
        opponent.WithUpgrade(UpgradeNames.BurningHands, upg =>
            opponent.AddEffect(new Effect(
                EffectType.WeaponStrenghten,
                2,
                false,
                upg.intensity
            )));
    }

    private static void HandleMaskOfMidnight(CombatantInstance combatant)
    {
        if (combatant is not DoobieInstance) return;

        GameManager.Instance.currentVangurr
            .WithUpgrade(UpgradeNames.MaskOfMidnight, upg =>
                combatant.AddEffect(new Effect(
                    EffectType.Holy,
                    2,
                    true,
                    upg.intensity
                )));
    }
}

/// <summary>
/// Handles spell-based upgrade triggers
/// </summary>
public static class SpellUpgradeHandler
{
    public static void HandleSpellUpgrade(CombatantInstance combatant, CombatantInstance opponent, Upgrade upgrade)
    {
        switch (upgrade.type)
        {
            case UpgradeNames.SpellSlinger:
                opponent.TakeDamage(upgrade.intensity, true);
                break;
            case UpgradeNames.SpellSorcerer:
                combatant.AddEffect(new Effect(EffectType.SpellStrenghten, 3, false, upgrade.intensity));
                break;
            case UpgradeNames.Shadowrend:
                if (combatant.CurrentHealth == combatant.MaxHealth)
                {
                    combatant.AddEffect(new Effect(EffectType.HealingStrenghten, 3, false, upgrade.intensity * 3));
                }
                break;
            case UpgradeNames.SpellsOfMenta:
                HandleSpellsOfMenta(combatant, opponent, upgrade);
                break;
        }
    }

    private static void HandleSpellsOfMenta(CombatantInstance combatant, CombatantInstance opponent, Upgrade upgrade)
    {
        Effect spellStrenghten = combatant.ActiveEffects.Find(ss => ss.type == EffectType.SpellStrenghten);
        if (spellStrenghten != null)
        {
            combatant.AddEffect(new Effect(EffectType.SpellStrenghten, 2, false, upgrade.intensity));
            opponent.AddEffect(new Effect(EffectType.Burn, 1, true, upgrade.intensity));
        }
    }
}

/// <summary>
/// Handles spell item triggers
/// </summary>
public static class SpellItemHandler
{
    public static void HandleSpellItem(CombatantInstance combatant, Item item, List<Effect> effectsSnapshot)
    {
        switch (item.type)
        {
            case ItemType.BleedingSpirit:
                HandleBleedingSpirit(combatant, effectsSnapshot);
                break;
            case ItemType.JarOfShadows:
                combatant.AddEffect(new Effect(EffectType.Shadow, 5, false, 1));
                break;
        }
    }

    private static void HandleBleedingSpirit(CombatantInstance combatant, List<Effect> effectsSnapshot)
    {
        foreach (var effect in effectsSnapshot)
        {
            if (effect.type == EffectType.Bleed)
            {
                var (result, damageDone) = combatant.TakeDamage(effect.intensity, false, true);
                BattleUIManager.Instance.AddLog($"{combatant.CharacterName} takes {damageDone} bleed damage!");
            }
        }
    }
}

/// <summary>
/// Handles weapon-based upgrade triggers
/// </summary>
public static class WeaponUpgradeHandler
{
    public static void HandleWeaponUpgrades(CombatantInstance combatant)
    {
        if (combatant.ActiveUpgrades == null) return;

        foreach (var upgrade in combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.WeaponMastery:
                    combatant.AddEffect(new Effect(EffectType.WeaponStrenghten, 1, false, upgrade.intensity));
                    break;
                case UpgradeNames.BloodyWeapon:
                    HandleBloodyWeapon(combatant, upgrade);
                    break;
                case UpgradeNames.ViolentAttacks:
                    HandleViolentAttacks(combatant, upgrade);
                    break;
                case UpgradeNames.OffensiveFlow:
                    HandleOffensiveFlow(combatant, upgrade);
                    break;
                case UpgradeNames.CalmRitual:
                    combatant.HealCombatant(upgrade.intensity);
                    break;
                case UpgradeNames.HeartOfStillness:
                    HandleHeartOfStillness(combatant, upgrade);
                    break;
                case UpgradeNames.SereneCarapace:
                    combatant.AddEffect(new Effect(EffectType.ConvertOverheal, 5, false, upgrade.intensity));
                    break;
            }
        }
    }

    private static void HandleBloodyWeapon(CombatantInstance combatant, Upgrade upgrade)
    {
        CombatantInstance target = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;
        target.AddEffect(new Effect(EffectType.Bleed, 3, true, upgrade.intensity));
    }

    private static void HandleViolentAttacks(CombatantInstance combatant, Upgrade upgrade)
    {
        combatant.AddEffect(new Effect(EffectType.Bleed, 2, true, 2));
        combatant.AddEffect(new Effect(EffectType.WeaponStrenghten, 3, true, upgrade.intensity));
    }

    private static void HandleOffensiveFlow(CombatantInstance combatant, Upgrade upgrade)
    {
        float chancePerIntensity = 0.05f;
        float totalChance = upgrade.intensity * chancePerIntensity;

        if (Random.value < totalChance)
        {
            combatant.AddEffect(new Effect(EffectType.Deflecion, 999, false, upgrade.intensity));
        }
    }

    private static void HandleHeartOfStillness(CombatantInstance combatant, Upgrade upgrade)
    {
        CombatantInstance target = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;
        target.AddEffect(new Effect(EffectType.NutouCurse, 1, true, upgrade.intensity));
    }
}