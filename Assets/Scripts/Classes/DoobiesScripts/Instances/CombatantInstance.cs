using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class CombatantInstance
{

    public UnityEngine.Transform animationAnchor;

    public abstract ScriptableObject so { get; }
    public abstract string CharacterName { get; }
    public abstract int CurrentHealth { get; set; }
    public abstract int MaxHealth { get; set; }
    public abstract float CurrentDefence { get; set; }
    public abstract int CurrentSkillDmg { get; set; }
    public abstract int CurrentHealPower { get; set; }

    public WeaponInstance EquippedWeaponInstance;

    public int GetEffectiveWeaponDamage() => EquippedWeaponInstance?.GetEffectiveDamage() ?? 0;
    public int GetEffectiveCritChance() => EquippedWeaponInstance?.GetEffectiveCritChance() ?? 0;

    public abstract List<SkillSO> GetAllSkills();
    public List<Effect> ActiveEffects { get; private set; } = new List<Effect>();
    public List<GameObject> ActiveEffectIcons = new List<GameObject>();

    public List<Upgrade> ActiveUpgrades { get; private set; } = new List<Upgrade>();

    public int HealCombatant(int amount)
    {
        int effectiveHeal = GetEffectiveHealPower(amount);

        effectiveHeal += CurrentHealPower;

        int healAmount = Mathf.Min(effectiveHeal, MaxHealth - CurrentHealth);
        CurrentHealth += healAmount;

        if (healAmount > 0)
        {
            CheckForOnHealEffects();
            if (this is VangurrInstance vangurr)
            {
                BattleUIManager.Instance.SpawnFloatingText("+" + healAmount, Color.green, BattleUIManager.Instance.VangurrHP.transform, false);
            }
            else
            {
                BattleUIManager.Instance.SpawnFloatingText("+" + healAmount, Color.green, BattleUIManager.Instance.DoobieHP.transform, false);
            }
        }

        BattleUIManager.Instance.AddLog($"{CharacterName} heals for {healAmount}!");

        return healAmount;
    }
    /// <summary>
    /// The instance takes damage, reduced by defence.
    /// </summary>
    /// <param name="amount">The amount of damage before defence</param>
    /// <param name="isSkill">wheter the dmg came from a skill</param>
    /// <returns>The actual damage</returns>
    public virtual (DamageResult result, int actualDamage) TakeDamage(int amount, bool isSkill = false, bool isEffect = false)
    {
        Debug.Log("Taking base damage: " + amount);

        if (HandleHidden())
        {
            BattleUIManager.Instance.AddLog($"{CharacterName} was hidden and avoided the damage!");
            return (DamageResult.Missed, 0);
        }

        if (HandleDeflection())
            return (DamageResult.Deflected, 0);

        // alleen melee / weapon attacks (niet skills) mogen dodges via Sneaky/Evasion doen
        if (!isSkill)
        {
            // 1) Als er al een Evasion-effect is, dit is een "Evasion dodge" -> trigger follow-ups
            if (HasEvasionEffect())
            {
                HandleDodgeEffects(); // grants WeaponStrenghten / TargetLocked etc.
                return (DamageResult.Dodged, 0);
            }

            // 2) Anders: probeer Sneaky te proccen. Als Sneaky slaagt, geef Evasion maar NIET de follow-ups nu.
            if (HandleSneaky())
            {
                return (DamageResult.Dodged, 0);
            }
        }

        // Normal damage calculation if no shield
        float defence = GetEffectiveDefence();
        int reducedDamage = Mathf.CeilToInt(amount / defence);

        if (HandleShield(reducedDamage))
            return (DamageResult.Blocked, 0);

        CurrentHealth = Mathf.Max(CurrentHealth - reducedDamage, 0);

        if (reducedDamage > 0)
        {
            HandleOnDamage(reducedDamage);
            if (this is VangurrInstance vangurr)
            {
                BattleUIManager.Instance.SpawnFloatingText("-" + reducedDamage, Color.red, BattleUIManager.Instance.VangurrHP.transform, true);
            }
            else
            {
                BattleUIManager.Instance.SpawnFloatingText("-" + reducedDamage, Color.red, BattleUIManager.Instance.DoobieHP.transform, true);
            }
        }

        // Trigger hit animation if available
        GameObject HitAnimationPrefab = GameManager.Instance.damageAnimationPrefab;
        if (HitAnimationPrefab != null)
        {
            PlayHitAnimation(HitAnimationPrefab);
        }

        return (DamageResult.Hit, reducedDamage);
    }
    private bool HandleDeflection()
    {
        var deflectEffects = ActiveEffects.FindAll(b => b.type == EffectType.Deflecion);

        if (deflectEffects.Count == 0)
            return false; // niks om te doen

        // Harden effect geven als er een sterke deflect was
        if (deflectEffects.Any(b => b.intensity >= 10))
        {
            AddEffect(new Effect(EffectType.Harden, 3, false, 3));
        }

        // Alle deflects weghalen
        ActiveEffects.RemoveAll(b => b.type == EffectType.Deflecion);

        // Check voor BloomBlossom ? herplaats deflection
        Effect bloomBlossomEffect = ActiveEffects.Find(b => b.type == EffectType.BloomBlossom);
        if (bloomBlossomEffect != null)
        {
            AddEffect(new Effect(EffectType.Deflecion, 999, false, 10));

            ActiveEffects.RemoveAll(b => b.type == EffectType.BloomBlossom);

            Upgrade ultimateBloomUpgrade = ActiveUpgrades.Find(b => b.type == UpgradeNames.UltimateBloom);
            if (ultimateBloomUpgrade != null)
            {
                for (int i = 0; i < ultimateBloomUpgrade.intensity; i++)
                {
                    AddEffect(new Effect(EffectType.WeaponStrenghten, 1, false, 1));
                }
            }
        }

        Upgrade deflectorUpgrade = ActiveUpgrades.Find(b => b.type == UpgradeNames.Deflector);
        if (deflectorUpgrade != null)
        {
            for (int i = 0; i < deflectorUpgrade.intensity; i++)
            {
                if (this is DoobieInstance)
                {
                    // deal 1 damage to vangurr for each deflectorupgrade intensity
                    GameManager.Instance.currentVangurr.CurrentHealth -= 1;
                }
                else 
                {
                    // deal 1 damage to doobie for each deflectorupgrade intensity
                    GameManager.Instance.currentDoobie.CurrentHealth -= 1;
                }
            }
        }

        return true; // een deflect is afgehandeld
    }
    private bool HandleSneaky()
    {
        int sneakyStacks = ActiveUpgrades.Count(u => u.type == UpgradeNames.Sneaky);
        if (sneakyStacks == 0) return false;

        float dodgeChance = sneakyStacks * 0.20f;

        if (Random.value < dodgeChance)
        {
            AddEffect(new Effect(EffectType.Evasion, sneakyStacks, false, sneakyStacks));

            return true;
        }

        return false;
    }
    private void HandleDodgeEffects()
    {
        var evasionEffect = ActiveEffects.Find(b => b.type == EffectType.Evasion);
        if (evasionEffect == null) return;

        // Voor elke stack (intensity) van Evasion: geef follow-up effects
        int stacks = Mathf.Max(1, evasionEffect.intensity); // defensive
        AddEffect(new Effect(EffectType.WeaponStrenghten, 2, false, stacks));
        AddEffect(new Effect(EffectType.CriticalEye, 2, false, stacks));

        evasionEffect.duration--;
        if (evasionEffect.duration <= 0)
            ActiveEffects.Remove(evasionEffect);
    }

    private bool HasEvasionEffect()
    {
        return ActiveEffects.Exists(b => b.type == EffectType.Evasion);
    }
    private bool HandleHidden()
    {
        Effect hiddenEffect = ActiveEffects.Find(b => b.type == EffectType.Hidden);
        if (hiddenEffect != null)
        {
            return true;
        }
        return false;
    }
    private bool HandleShield(int damage)
    {
        Effect shieldEffect = ActiveEffects.Find(b => b.type == EffectType.Shield);
        if (shieldEffect != null && shieldEffect.intensity > 0)
        {
            shieldEffect.intensity -= damage;

            if (shieldEffect.intensity <= 0)
            {
                ActiveEffects.Remove(shieldEffect);
            }

            return true;
        }

        return false;
    }
    private bool HandleOnDamage(int damage)
    {
        Effect vampireCurse = ActiveEffects.Find(b => b.type == EffectType.VampireCurse);
        if (vampireCurse != null)
        {
            for (int i = 0; i < vampireCurse.intensity; i++)
            {
                int healAmount = Mathf.CeilToInt(0.5f * damage);
                if (this is DoobieInstance)
                {
                    GameManager.Instance.currentVangurr.HealCombatant(healAmount);
                }
                else
                {
                    GameManager.Instance.currentDoobie.HealCombatant(healAmount);
                }
            }
            return true;
        }

        foreach (Upgrade upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.TargetFound:
                    if (this is DoobieInstance)
                    {
                        GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.TargetLocked, 2, true, upgrade.intensity));
                    }
                    else
                    {
                        GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.TargetLocked, 2, true, upgrade.intensity));
                    }
                    break;
                default:
                    break;
            }
        }

        return false;
    }


    /// <summary>
    /// The instance preforms a basic attack and deals damage to the target if posible
    /// </summary>
    /// <param name="target">The instance that is getting attacked</param>
    /// <returns>String that the combat log needs</returns>
    public virtual string PerformBasicAttack(CombatantInstance target)
    {
        if (EquippedWeaponInstance == null)
            return $"{CharacterName} tries to attack, but is unarmed!";

        var attack = EquippedWeaponInstance.GetEffectiveDamage();

        Effect BlindDeEffect = ActiveEffects.Find(b => b.type == EffectType.Blind);
        if (Random.value < EquippedWeaponInstance.MissChance || BlindDeEffect != null)
        {
            return $"{CharacterName} swings at {target.CharacterName}, but misses!";
        }

        float multiplier = Random.Range(0.5f, 1.5f);

        int baseDamage = Mathf.RoundToInt(attack * multiplier);

        // Apply any attack-affecting effects
        int baseDamageAfterEffects = GetEffectiveWeaponDamageAfterEffects(baseDamage);

        bool isCrit = Random.Range(0, 100) < GetEffectiveCritChanceAfterEffects(GetEffectiveCritChance());

        int damageBeforeCrit = baseDamageAfterEffects;

        if (isCrit)
        {
            int damageAfterCrit = ApplyCriticalHitEffects(baseDamageAfterEffects);

            damageBeforeCrit = damageAfterCrit;
        }

        int finalDamage = damageBeforeCrit;

        // Activate Effects
        if (EquippedWeaponInstance.Animation != null)
        {
            target.PlayAttackAnimation(EquippedWeaponInstance.Animation);
        }

        var (result, actualDamage) = target.TakeDamage(finalDamage);

        // Apply all upgrade effects
        ApplyEffectsOnBasicAttack(target);
        CheckForWeaponOnUseEffects();

        switch (result)
        {
            case DamageResult.Deflected:
                return $"{CharacterName} strikes, but {target.CharacterName} deflects the blow with finesse!";
            case DamageResult.Hit:
                return isCrit
                    ? $"{CharacterName} lands a CRITICAL HIT on {target.CharacterName} for {actualDamage} damage!"
                    : $"{CharacterName} strikes {target.CharacterName} for {actualDamage} damage!";
            case DamageResult.Missed:
                return $"{CharacterName}'s attack phases through thin air!";
            case DamageResult.Immune:
                return $"{target.CharacterName} is immune to the attack!";
            case DamageResult.Blocked:
                return $"{target.CharacterName} blocks the hit and takes no damage!";
            case DamageResult.Dodged:
                return $"{target.CharacterName} swiftly dodges the attack!";
            default:
                return $"{CharacterName} attacks, but something strange happens...";
        }
    }

    public int ApplyCriticalHitEffects(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.CriticalMonster:
                    AddEffect(new Effect(EffectType.CriticalEye, 3, false, upgrade.intensity));

                    modifiedDamage += upgrade.intensity + 1;
                    break;
                default:
                    break;
            }
        }
        return modifiedDamage;
    }
    public int GetEffectiveWeaponDamageAfterEffects(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var effect in ActiveEffects)
        {
            switch (effect.type)
            {
                case EffectType.WeaponWeaken:
                    for (int i = 0; i < effect.intensity; i++)
                        modifiedDamage = Mathf.FloorToInt(modifiedDamage * 0.8f);
                    break;
                case EffectType.WeaponStrenghten:
                    for (int i = 0; i < effect.intensity; i++)
                        modifiedDamage = Mathf.CeilToInt(modifiedDamage * 1.2f);
                    break;
            }
        }

        return Mathf.Max(modifiedDamage, 0);
    }
    public int GetEffectiveWeaponDamageAfterEffectsForUI(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var effect in ActiveEffects)
        {
            switch (effect.type)
            {
                case EffectType.WeaponWeaken:
                    for (int i = 0; i < effect.intensity; i++)
                        modifiedDamage = Mathf.FloorToInt(modifiedDamage * 0.8f);
                    break;
                case EffectType.WeaponStrenghten:
                    for (int i = 0; i < effect.intensity; i++)
                        modifiedDamage = Mathf.CeilToInt(modifiedDamage * 1.2f);
                    break;
            }
        }

        return Mathf.Max(modifiedDamage, 0);
    }
    public int GetEffectiveSkillDamageAfterEffects(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var effect in ActiveEffects)
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
        return Mathf.Max(modifiedDamage, 0);
    }
    public int GetEffectiveCritChanceAfterEffects(int baseCrit)
    {
        int modifiedCrit = baseCrit;
        foreach (var effect in ActiveEffects)
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

    public int GetEffectiveSkillDamage(int baseDmg)
    {
        int finalDmg;

        CheckForSkillOnUseEffects();

        return finalDmg = GetEffectiveSkillDamageAfterEffects(baseDmg);
    }
    public int GetEffectiveSkillDamageForUI(int baseDmg)
    {
        int finalDmg;

        return finalDmg = GetEffectiveSkillDamageAfterEffects(baseDmg);
    }
    public int GetEffectiveHealPower(int baseHeal)
    {
        float modifiedHeal = baseHeal;

        foreach (var effect in ActiveEffects)
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

        return Mathf.Max(0, Mathf.RoundToInt(modifiedHeal));
    }


    public void CheckForSkillOnUseEffects()
    {
        if (ActiveUpgrades == null) return;
        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.SpellSlinger:
                    if (this is DoobieInstance)
                    {
                        GameManager.Instance.currentVangurr.TakeDamage(upgrade.intensity);
                    }
                    else
                    {
                        GameManager.Instance.currentDoobie.TakeDamage(upgrade.intensity);
                    }
                    BattleUIManager.Instance.AddLog("Spellslinger Activates!");
                    break;
                case UpgradeNames.SpellSorcerer:
                     AddEffect(new Effect(EffectType.SpellStrenghten, 3 ,false, upgrade.intensity));
                    break;
                default:
                    break;
            }
        }
    }
    public void CheckForWeaponOnUseEffects()
    {
        if (ActiveUpgrades != null)
        {
            foreach (var upgrade in ActiveUpgrades)
            {
                switch (upgrade.type)
                {
                    case UpgradeNames.WeaponMastery:
                        AddEffect(new Effect(EffectType.WeaponStrenghten, 1, false, upgrade.intensity));
                        break;
                    case UpgradeNames.BloodyWeapon:
                        if (this is DoobieInstance)
                        {
                            GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Bleed, 3, true, upgrade.intensity));
                        }
                        else
                        {
                            GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.Bleed, 3, true, upgrade.intensity));
                        }
                        break;
                    case UpgradeNames.ViolentAttacks:
                        if (this is DoobieInstance)
                        {
                            GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.Bleed, 2, true, 2));

                            GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.WeaponStrenghten, 3, true, upgrade.intensity));
                        }
                        else
                        {
                            GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Bleed, 2, true, 2));

                            GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.WeaponStrenghten, 3, true, upgrade.intensity));
                        }
                        break;
                    case UpgradeNames.OffensiveFlow:
                        // Each intensity adds 5% chance to gain 1 Deflection
                        float chancePerIntensity = 0.05f;
                        float totalChance = upgrade.intensity * chancePerIntensity;

                        if (Random.value < totalChance)
                        {
                            AddEffect(new Effect(EffectType.Deflecion, 999, false, upgrade.intensity));
                        }
                        break;
                }
            }
        }
        if (ActiveEffects != null)
        {
            var effectsSnapshot = new List<Effect>(ActiveEffects);

            for (int i = 0; i < effectsSnapshot.Count; i++)
            {
                var effect = effectsSnapshot[i];
                switch (effect.type)
                {
                    case  EffectType.Bleed:
                        for (int j = 0; j < effect.intensity; j++)
                        {
                            var (result, damageDone) = TakeDamage(1, false);
                            BattleUIManager.Instance.AddLog($"{CharacterName} takes {damageDone} bleed damage!");
                        }
                        break;
                    case EffectType.Enflame:
                        if (this is DoobieInstance)
                        {
                            GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Burn, 2, true, effect.intensity));
                        }
                        else
                        {
                            GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.Burn, 2, true, effect.intensity));
                        }
                        break;
                    default:
                        break;
                }
            }
        }

    }
    public void CheckForOnHealEffects()
    {
        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.FlowersOfRot:
                    AddEffect(new Effect(EffectType.HealingStrenghten, 1, false, upgrade.intensity));
                    if (this is DoobieInstance)
                    {
                        GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.TargetLocked, 2, true, upgrade.intensity));
                    }
                    else
                    {
                        GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.TargetLocked, 2, true, upgrade.intensity));
                    }
                    break;
                case UpgradeNames.FireFlies:
                    if (this is DoobieInstance)
                    {
                        GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Burn, 2, true, upgrade.intensity));
                    }
                    else
                    {
                        GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.Burn, 2, true, upgrade.intensity));
                    }
                    break;
            }
        }
    }

    protected void ApplyEffectsOnBasicAttack(CombatantInstance target)
    {
        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.Firebrand:
                    target.AddEffect(new Effect(EffectType.Burn, 3, true, upgrade.intensity)); 
                    break;
            }
        }
        foreach (var effect in ActiveEffects)
        {
            switch (effect.type)
            {
                case EffectType.Barrel:
                    if (this is DoobieInstance)
                    {
                        var (result, damageDone) = GameManager.Instance.currentVangurr.TakeDamage(effect.intensity, false, true);
                        BattleUIManager.Instance.AddLog($"{CharacterName} explodes the {effect.intensity} barrels!");
                    }
                    else
                    {
                        var (result, damageDone) = GameManager.Instance.currentDoobie.TakeDamage(effect.intensity, false, true);
                        BattleUIManager.Instance.AddLog($"{GameManager.Instance.currentVangurr.CharacterName} explodes the {effect.intensity} barrels!");
                    }
                        break;
                default:
                    break;
            }
        }
    }
    private void AddEffectUpgradesCheck(Effect newEffect)
    {
        if (newEffect.type == EffectType.Deflecion)
        {
            Upgrade fleetingPetalsUpgrade = ActiveUpgrades.Find(b => b.type == UpgradeNames.FleetingPetals);
            if (fleetingPetalsUpgrade != null)
            {
                for (int i = 0; i < fleetingPetalsUpgrade.intensity; i++)
                {
                    HealCombatant(1);
                }
            }

            Upgrade whiteFlowerUpgrade = ActiveUpgrades.Find(b => b.type == UpgradeNames.WhiteFlower);
            if (whiteFlowerUpgrade != null)
            {
                if (this is DoobieInstance doobie && doobie.MainResource != null && doobie.MainResource.Type == ResourceType.Zurp)
                {
                    doobie.MainResource.Gain(whiteFlowerUpgrade.intensity);
                }
            }
        }

        if (newEffect.type == EffectType.SpellWeaken)
        {
            Upgrade powerSpells = ActiveUpgrades.Find(b => b.type == UpgradeNames.PowerSpells);
            if (powerSpells != null)
            {
                Effect spellWeaken = ActiveEffects.Find(s => s.type == EffectType.SpellWeaken);
                if (spellWeaken != null)
                {
                    spellWeaken.duration -= powerSpells.intensity;
                    spellWeaken.intensity -= powerSpells.intensity;
                }
            }
        }
    }
    public void KillInstance()
    {
        CurrentHealth = 0;
    }
    public void AddEffect(Effect newEffect)
    {
        // Special case: TargetLocked should NOT stack
        if (newEffect.type == EffectType.TargetLocked)
        {
            ActiveEffects.Add(newEffect);
        }
        else
        {
            // For all other effects, stack if already exists
            Effect existing = ActiveEffects.Find(b => b.type == newEffect.type);

            if (existing != null)
            {
                existing.duration += newEffect.duration;
                existing.intensity += newEffect.intensity;

                // Play stacking effect
                if (existing.iconInstance != null)
                    existing.iconInstance.PlayEffect();
            }
            else
            {
                ActiveEffects.Add(newEffect);
            }
        }

        AddEffectUpgradesCheck(newEffect);

        UnityEngine.Transform effectContainer = this is DoobieInstance
            ? BattleUIManager.Instance.DoobieEffectsContainer
            : BattleUIManager.Instance.VangurrEffectsContainer;

        BattleUIManager.Instance.UpdateEffectsUI(this, effectContainer);

        // --- Add quick combat log entry ---
        string effectName = newEffect.iconGO != null ? newEffect.iconGO.name : newEffect.type.ToString();
        string logMessage = $"{CharacterName} gains {newEffect.intensity} \"{effectName}\"!";
        BattleUIManager.Instance.AddLog(logMessage);
    }


    public void AddUpgrade(Upgrade newUpgrade)
    {
        Upgrade existing = ActiveUpgrades.Find(b => b.type == newUpgrade.type);
        if (existing != null)
        {
            existing.intensity += newUpgrade.intensity;
        }
        else
        {
            ActiveUpgrades.Add(newUpgrade);
        }
    }

    public void TickEffects()
    {
        for (int i = ActiveEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffects[i].duration--;

            if (ActiveEffects[i].duration <= 0)
            {
                if (ActiveEffects[i].type == EffectType.TargetLocked)
                {
                    var (result, damageDone) = TakeDamage(ActiveEffects[i].intensity);
                    BattleUIManager.Instance.AddLog($"Target Locked activates! dealing {damageDone} damage!");
                }

                ActiveEffects.RemoveAt(i);
            }
        }

        // Immediately refresh UI so you never show duration=0
        BattleUIManager.Instance.UpdateEffectsUI(this,
            this is DoobieInstance
                ? BattleUIManager.Instance.DoobieEffectsContainer
                : BattleUIManager.Instance.VangurrEffectsContainer
        );
    }

    public float GetEffectiveDefence()
    {
        float defence = CurrentDefence;

        foreach (var Effects in ActiveEffects)
        {
            if (Effects.type == EffectType.DefenceDown)
            {
                for (int i = 0; i < Effects.intensity; i++)
                    defence *= 0.8f;
            }
            else if (Effects.type == EffectType.Harden)
            {
                for (int i = 0; i < Effects.intensity; i++)
                    defence *= 1.2f;
            }
        }

        return defence;
    }

    /// <summary>
    /// Activate the animation of the weapon
    /// </summary>
    /// <param name="animationPrefab">The animation prefab</param>
    public void PlayAttackAnimation(GameObject animationPrefab)
    {
        if (animationPrefab == null || animationAnchor == null)
            return;

        GameObject spawned = GameObject.Instantiate(animationPrefab, animationAnchor.position, Quaternion.identity);
        spawned.transform.SetParent(animationAnchor);
        spawned.transform.localScale.Normalize();

        var ps = spawned.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var renderer = ps.GetComponent<Renderer>();
            renderer.sortingLayerName = "Foreground";
            renderer.sortingOrder = 10;
        }

        GameObject.Destroy(spawned, 2f);
    }
    /// <summary>
    /// Play a hit/damage animation on this combatant.
    /// </summary>
    /// <param name="animationPrefab">The animation prefab to play</param>
    public void PlayHitAnimation(GameObject animationPrefab)
    {
        if (animationPrefab == null || animationAnchor == null)
            return;

        GameObject spawned = GameObject.Instantiate(animationPrefab, animationAnchor.position, Quaternion.identity);
        spawned.transform.SetParent(animationAnchor);
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
}

