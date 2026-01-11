using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// Handles deflection effect logic
/// </summary>
public static class DeflectionHandler
{
    public static void Handle(CombatantInstance combatant, List<Effect> deflectEffects)
    {
        if (deflectEffects.Any(b => b.intensity >= 10))
        {
            combatant.AddEffect(new Effect(EffectType.Harden, 3, false, 3));
        }

        combatant.ActiveEffects.RemoveAll(b => b.type == EffectType.Deflecion);

        Effect bloomBlossomEffect = combatant.ActiveEffects.Find(b => b.type == EffectType.BloomBlossom);
        if (bloomBlossomEffect != null)
        {
            HandleBloomBlossom(combatant);
        }

        HandleDeflectorUpgrade(combatant);
        HandleDeflectionItems(combatant);
    }

    private static void HandleBloomBlossom(CombatantInstance combatant)
    {
        combatant.AddEffect(new Effect(EffectType.Deflecion, 999, false, 10));
        combatant.ActiveEffects.RemoveAll(b => b.type == EffectType.BloomBlossom);

        Upgrade ultimateBloomUpgrade = combatant.ActiveUpgrades.Find(b => b.type == UpgradeNames.UltimateBloom);
        if (ultimateBloomUpgrade != null)
        {
            for (int i = 0; i < ultimateBloomUpgrade.intensity; i++)
            {
                combatant.AddEffect(new Effect(EffectType.WeaponStrenghten, 1, false, 1));
            }
        }
    }

    private static void HandleDeflectorUpgrade(CombatantInstance combatant)
    {
        Upgrade deflectorUpgrade = combatant.ActiveUpgrades.Find(b => b.type == UpgradeNames.Deflector);
        if (deflectorUpgrade == null) return;

        CombatantInstance target = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;

        for (int i = 0; i < deflectorUpgrade.intensity; i++)
        {
            target.CurrentHealth -= 1;
        }
    }

    private static void HandleDeflectionItems(CombatantInstance combatant)
    {
        foreach (Item item in combatant.ActiveItems)
        {
            switch (item.type)
            {
                case ItemType.StrikingFlower:
                    combatant.AddEffect(new Effect(EffectType.BloomBlossom, 2, false, 1));
                    break;
            }
        }
    }
}

/// <summary>
/// Handles shield-type effects
/// </summary>
public static class ShieldHandler
{
    public static bool HandleShield(CombatantInstance combatant, int damage)
    {
        if (HandleRegularShield(combatant, damage)) return true;
        if (HandleBlessedShield(combatant, damage)) return true;
        return false;
    }

    private static bool HandleRegularShield(CombatantInstance combatant, int damage)
    {
        Effect shieldEffect = combatant.ActiveEffects.Find(b => b.type == EffectType.Shield);
        if (shieldEffect == null || shieldEffect.intensity <= 0) return false;

        shieldEffect.intensity -= damage;
        if (shieldEffect.intensity <= 0)
        {
            combatant.ActiveEffects.Remove(shieldEffect);
        }
        return true;
    }

    private static bool HandleBlessedShield(CombatantInstance combatant, int damage)
    {
        Effect blessedShieldEffect = combatant.ActiveEffects.Find(b => b.type == EffectType.BlessedShield);
        if (blessedShieldEffect == null || blessedShieldEffect.intensity <= 0) return false;

        blessedShieldEffect.intensity -= damage;
        if (blessedShieldEffect.intensity <= 0)
        {
            combatant.ActiveEffects.Remove(blessedShieldEffect);
            combatant.AddEffect(new Effect(EffectType.HealingStrenghten, 5, false, combatant.CurrentHealPower));
        }
        return true;
    }
}

/// <summary>
/// Handles on-damage triggers
/// </summary>
public static class OnDamageHandler
{
    public static void Handle(CombatantInstance combatant, int damage, bool isSkill, SkillSO skill = null)
    {
        HandleEffectTriggers(combatant, damage, isSkill, skill);
        HandleUpgradeTriggers(combatant, damage, isSkill, skill);
        HandleOpponentUpgrades(combatant, damage, isSkill, skill);
    }

    private static void HandleEffectTriggers(CombatantInstance combatant, int damage, bool isSkill, SkillSO skill = null)
    {
        HandleVampireCurse(combatant, damage);
        HandleNutouCurse(combatant);
        HandleCrimsonCurse(combatant);
        HandleCrystalize(combatant);
        HandleConfused(combatant, isSkill, skill);
    }

    private static void HandleVampireCurse(CombatantInstance combatant, int damage)
    {
        Effect vampireCurse = combatant.ActiveEffects.Find(b => b.type == EffectType.VampireCurse);
        if (vampireCurse == null) return;

        for (int i = 0; i < vampireCurse.intensity; i++)
        {
            int healAmount = Mathf.CeilToInt(0.5f * damage);
            CombatantInstance healer = combatant is DoobieInstance
                ? GameManager.Instance.currentVangurr
                : GameManager.Instance.currentDoobie;
            healer.HealCombatant(healAmount);
        }
    }

    private static void HandleNutouCurse(CombatantInstance combatant)
    {
        Effect nutouCurse = combatant.ActiveEffects.Find(b => b.type == EffectType.NutouCurse);
        if (nutouCurse != null)
        {
            combatant.AddEffect(new Effect(EffectType.HealingWeaken, 1, true, nutouCurse.intensity));
        }
    }

    private static void HandleCrimsonCurse(CombatantInstance combatant)
    {
        Effect crimsonCurse = combatant.ActiveEffects.Find(c => c.type == EffectType.CrimsonCurse);
        if (crimsonCurse != null)
        {
            combatant.AddEffect(new Effect(EffectType.Burn, 1, true, crimsonCurse.intensity));
        }
    }

    private static void HandleCrystalize(CombatantInstance combatant)
    {
        Effect crystalize = combatant.ActiveEffects.Find(c => c.type == EffectType.Crystalize);
        if (crystalize != null)
        {
            Effect hardenEffect = combatant.ActiveEffects.Find(h => h.type == EffectType.Harden);
            if (hardenEffect != null)
            {
                combatant.ActiveEffects.Remove(hardenEffect);
            }
        }
    }

    private static void HandleConfused(CombatantInstance combatant, bool isSkill, SkillSO skill = null)
    {
        Effect confused = combatant.ActiveEffects.Find(c => c.type == EffectType.Confused);
        if (confused != null && isSkill)
        {
            combatant.AddEffect(new Effect(EffectType.Stun, 2, true, confused.intensity));
            combatant.ActiveEffects.Remove(confused);
        }
    }

    private static void HandleUpgradeTriggers(CombatantInstance combatant, int damage, bool isSkill = false, SkillSO skill = null)
    {
        foreach (Upgrade upgrade in combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.TargetFound:
                    CombatantInstance target = combatant is DoobieInstance
                        ? GameManager.Instance.currentVangurr
                        : GameManager.Instance.currentDoobie;
                    target.AddEffect(new Effect(EffectType.TargetLocked, 2, true, upgrade.intensity));
                    break;
            }
        }
    }

    private static void HandleOpponentUpgrades(CombatantInstance combatant, int damage, bool isSkill = false, SkillSO skill = null)
    {
        CombatantInstance opponent = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;

        foreach (Upgrade opponentUpgrade in opponent.ActiveUpgrades)
        {
            switch (opponentUpgrade.type)
            {
                case UpgradeNames.BoneSnapper:
                    if (combatant.CurrentHealth != combatant.MaxHealth &&
                        combatant.CurrentTransformation == Transformations.SpiritForm)
                    {
                        combatant.AddEffect(new Effect(EffectType.Bleed, 2, true, opponentUpgrade.intensity));
                    }
                    break;
                case UpgradeNames.FireConstruct:
                    if (isSkill && skill.isWeaponSkill == false)
                    {
                        int burnGain = damage / 5;

                        for (int i = 0; i < opponentUpgrade.intensity; i++)
                        {
                            combatant.AddEffect(new Effect(EffectType.Burn, 2, true, burnGain));
                        }
                    }
                    break;
                case UpgradeNames.FlameOfMenta:
                    if (isSkill && skill.isWeaponSkill == false && combatant.HasEffect(EffectType.Burn))
                    {
                        for (int i = 0; i < opponentUpgrade.intensity; i++)
                        {
                            combatant.AddEffect(new Effect(EffectType.Burn, 2, true, 2));
                            if (opponent.HasEffect(EffectType.SpellStrenghten))
                            {
                                combatant.AddEffect(new Effect(EffectType.SpellWeaken, 3, true, 5));
                            }
                        }
                    }
                    break;
            }
        }
    }
}

/// <summary>
/// Handles TargetLocked expiration
/// </summary>
public static class TargetLockedHandler
{
    public static void Activate(CombatantInstance combatant, Effect expired)
    {
        var (result, damageDone) = combatant.TakeDamage(expired.intensity, true, true);
        BattleUIManager.Instance.AddLog($"Target Locked activates! dealing {damageDone} damage!");

        HandleTargetGardenSynergy(combatant, expired);
        HandleTargetScopedItem(combatant, expired);
    }

    private static void HandleTargetGardenSynergy(CombatantInstance combatant, Effect expired)
    {
        CombatantInstance opponent = combatant.GetOpponent();
        var opponentUpgrade = opponent.ActiveUpgrades.Find(u => u.type == UpgradeNames.TargetGarden);
        if (opponentUpgrade != null)
        {
            opponent.AddEffect(new Effect(EffectType.Regeneration, 2, true, opponentUpgrade.intensity));
        }
    }

    private static void HandleTargetScopedItem(CombatantInstance combatant, Effect expired)
    {
        CombatantInstance opponent = combatant.GetOpponent();
        var opponentTargetScoped = opponent.ActiveItems.Find(u => u.type == ItemType.TargetScoped);
        if (opponentTargetScoped != null)
        {
            opponent.AddEffect(new Effect(EffectType.TargetLocked, expired.duration + 1, true, expired.intensity));
            BattleUIManager.Instance.AddLog($"TargetScoped");
        }
    }
}

/// <summary>
/// Handles TimedBomb expiration
/// </summary>
public static class TimedBombHandler
{
    public static void Activate(CombatantInstance combatant, Effect expired)
    {
        CombatantInstance caster = combatant.GetOpponent();
        int baseDmg = caster.GetEffectiveSkillDamage(caster.CurrentSkillDmg);
        baseDmg *= expired.intensity;

        combatant.TakeDamage(baseDmg, true, true);
        BattleUIManager.Instance.AddLog($"{combatant.CharacterName}'s Timed Bomb explodes for {baseDmg} damage!");
    }
}

/// <summary>
/// Handles weapon effect triggers
/// </summary>
public static class WeaponEffectHandler
{
    public static void HandleWeaponEffects(CombatantInstance combatant)
    {
        if (combatant.ActiveEffects == null) return;

        var effectsSnapshot = new List<Effect>(combatant.ActiveEffects);

        foreach (var effect in effectsSnapshot)
        {
            switch (effect.type)
            {
                case EffectType.Bleed:
                    HandleBleed(combatant, effect);
                    break;
                case EffectType.Enflame:
                    HandleEnflame(combatant, effect);
                    break;
                case EffectType.HardHitter:
                    HandleHardHitter(combatant, effect);
                    break;
            }
        }
    }

    private static void HandleBleed(CombatantInstance combatant, Effect effect)
    {
        var (result, damageDone) = combatant.TakeDamage(effect.intensity, false, true);
        BattleUIManager.Instance.AddLog($"{combatant.CharacterName} takes {damageDone} bleed damage!");
    }

    private static void HandleEnflame(CombatantInstance combatant, Effect effect)
    {
        CombatantInstance target = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;
        target.AddEffect(new Effect(EffectType.Burn, 2, true, effect.intensity));
    }

    private static void HandleHardHitter(CombatantInstance combatant, Effect effect)
    {
        int bonusChance = 10 * effect.intensity;
        int baseCrit = combatant.GetEffectiveCritChance();
        int totalChance = combatant.GetEffectiveCritChanceAfterEffects(baseCrit + bonusChance) - 5;

        if (Random.Range(0, 100) < totalChance)
        {
            CombatantInstance target = combatant is DoobieInstance
                ? GameManager.Instance.currentVangurr
                : GameManager.Instance.currentDoobie;
            target.AddEffect(new Effect(EffectType.Stun, 1, true, effect.intensity));
        }
    }
}

/// <summary>
/// Handles heal-based upgrade triggers
/// </summary>
public static class HealUpgradeHandler
{
    public static void HandleHealUpgrades(CombatantInstance combatant)
    {
        foreach (var upgrade in combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.FlowersOfRot:
                    HandleFlowersOfRot(combatant, upgrade);
                    break;
                case UpgradeNames.FireFlies:
                    HandleFireFlies(combatant, upgrade);
                    break;
                case UpgradeNames.VineLash:
                    HandleVineLash(combatant, upgrade);
                    break;
                case UpgradeNames.HealingFaith:
                    HandleHealingFaith(combatant, upgrade);
                    break;
                case UpgradeNames.IronBreath:
                    HandleIronBreath(combatant, upgrade);
                    break;
            }
        }
    }

    private static void HandleFlowersOfRot(CombatantInstance combatant, Upgrade upgrade)
    {
        combatant.AddEffect(new Effect(EffectType.HealingStrenghten, 1, false, upgrade.intensity));
        CombatantInstance target = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;
        target.AddEffect(new Effect(EffectType.TargetLocked, 2, true, upgrade.intensity));
    }

    private static void HandleFireFlies(CombatantInstance combatant, Upgrade upgrade)
    {
        CombatantInstance target = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;
        target.AddEffect(new Effect(EffectType.Burn, 2, true, upgrade.intensity));
    }

    private static void HandleVineLash(CombatantInstance combatant, Upgrade upgrade)
    {
        CombatantInstance target = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;
        target.AddEffect(new Effect(EffectType.Vines, 2, true, upgrade.intensity));
    }

    private static void HandleHealingFaith(CombatantInstance combatant, Upgrade upgrade)
    {
        if (combatant is DoobieInstance doobie && doobie.CurrentGoddess == GoddessType.Elenara)
        {
            doobie.MainResource.Gain(upgrade.intensity);
            BattleUIManager.Instance.AddLog($"{combatant.CharacterName} has gained 2 Faith!");
        }
    }

    private static void HandleIronBreath(CombatantInstance combatant, Upgrade upgrade)
    {
        if (combatant.CurrentHealth >= (combatant.MaxHealth / 2))
        {
            combatant.AddEffect(new Effect(EffectType.HealingStrenghten, 3, false, upgrade.intensity));
        }
    }
}

/// <summary>
/// Handles heal effect triggers
/// </summary>
public static class HealEffectHandler
{
    public static void HandleHealEffects(CombatantInstance combatant)
    {
        var effectsSnapshot = new List<Effect>(combatant.ActiveEffects);

        foreach (var effect in effectsSnapshot)
        {
            switch (effect.type)
            {
                case EffectType.Vines:
                    BattleUIManager.Instance.AddLog($"{combatant.CharacterName} {effect.intensity} vines activate!");
                    combatant.TakeDamage(effect.intensity, true);
                    break;
            }
        }
    }
}

/// <summary>
/// Handles overheal upgrade triggers
/// </summary>
public static class OverhealUpgradeHandler
{
    public static void HandleOverhealUpgrades(CombatantInstance combatant)
    {
        foreach (var upgrade in combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.OverflowingGrace:
                    combatant.AddEffect(new Effect(EffectType.Regeneration, 1, false, upgrade.intensity));
                    break;
            }
        }
    }
}

/// <summary>
/// Handles overheal effect triggers
/// </summary>
public static class OverhealEffectHandler
{
    public static void HandleOverhealEffects(CombatantInstance combatant)
    {
        var effectsSnapshot = new List<Effect>(combatant.ActiveEffects);

        foreach (var effect in effectsSnapshot)
        {
            switch (effect.type)
            {
                case EffectType.ConvertOverheal:
                    combatant.AddEffect(new Effect(EffectType.Shield, 10, false, effect.intensity));
                    break;
            }
        }
    }
}

/// <summary>
/// Handles attack-based upgrade triggers
/// </summary>
public static class AttackUpgradeHandler
{
    public static void HandleAttackUpgrades(CombatantInstance combatant)
    {
        foreach (var upgrade in combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.BattleFaith:
                    HandleBattleFaith(combatant, upgrade);
                    break;
                case UpgradeNames.SpearOfRadiance:
                    HandleSpearOfRadiance(combatant, upgrade);
                    break;
                case UpgradeNames.EchoExplosion:
                    HandleEchoExplosion(combatant, upgrade);
                    break;
            }
        }
    }

    private static void HandleBattleFaith(CombatantInstance combatant, Upgrade upgrade)
    {
        if (combatant is DoobieInstance doobie && doobie.CurrentGoddess == GoddessType.Kaelyth)
        {
            doobie.MainResource.Gain(upgrade.intensity);
            BattleUIManager.Instance.AddLog($"{combatant.CharacterName} has gained 2 Faith!");
        }
    }

    private static void HandleSpearOfRadiance(CombatantInstance combatant, Upgrade upgrade)
    {
        bool hasDebuff = false;
        foreach (var effect in combatant.ActiveEffects)
        {
            if (effect.isDebuff)
            {
                hasDebuff = true;
                break;
            }
        }

        if (hasDebuff)
        {
            CombatantInstance target = combatant is DoobieInstance
                ? GameManager.Instance.currentVangurr
                : GameManager.Instance.currentDoobie;
            target.TakeDamage(upgrade.intensity * 3, true);
        }
    }

    private static void HandleEchoExplosion(CombatantInstance combatant, Upgrade upgrade)
    {
        CombatantInstance target = combatant is DoobieInstance
            ? GameManager.Instance.currentVangurr
            : GameManager.Instance.currentDoobie;
        target.AddEffect(new Effect(EffectType.TimedBomb, 5, true, upgrade.intensity));
    }
}

/// <summary>
/// Handles burn damage reactions
/// </summary>
public static class BurnDamageHandler
{
    public static void HandleBurnDamage(CombatantInstance combatant, CombatantInstance opponent, int damage)
    {
        HandleBurnUpgrades(opponent);
        HandleBurnItems(combatant, opponent);
    }

    private static void HandleBurnUpgrades(CombatantInstance opponent)
    {
        foreach (var upgrade in opponent.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.WalkThePlank:
                    opponent.AddEffect(new Effect(EffectType.Barrel, 100, false, upgrade.intensity));
                    BattleUIManager.Instance.AddLog($"{opponent.CharacterName}'s Walk The Plank creates a Barrel!");
                    break;
            }
        }
    }

    private static void HandleBurnItems(CombatantInstance combatant, CombatantInstance opponent)
    {
        foreach (Item item in opponent.ActiveItems)
        {
            if (item.type == ItemType.Fuel)
            {
                var burnEffects = combatant.ActiveEffects.FindAll(e => e.type == EffectType.Burn);
                foreach (var burn in burnEffects)
                {
                    burn.intensity *= 2;
                }
                BattleUIManager.Instance.AddLog($"{opponent.CharacterName}'s Fuel item doubles {combatant.CharacterName}'s burn intensity!");
            }
        }
    }
}