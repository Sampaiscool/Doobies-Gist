using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.RuleTile.TilingRuleOutput;

public abstract class CombatantInstance
{

    public UnityEngine.Transform animationAnchor;

    /// <summary>
    /// So of the Instance
    /// </summary>
    public abstract ScriptableObject so { get; }
    /// <summary>
    /// Name of the Instance
    /// </summary>
    public abstract string CharacterName { get; }
    /// <summary>
    /// Current Image of the Instance
    /// </summary>
    public abstract Sprite CurrentImage { get; set; }
    /// <summary>
    /// Current health of the Instance
    /// </summary>
    public abstract int CurrentHealth { get; set; }
    /// <summary>
    /// Max health of the Instance
    /// </summary>
    public abstract int MaxHealth { get; set; }
    /// <summary>
    /// Current Defence of the Instance
    /// </summary>
    public abstract float CurrentDefence { get; set; }
    /// <summary>
    /// Current skill/spell damage of the Instance
    /// </summary>
    public abstract int CurrentSkillDmg { get; set; }
    /// <summary>
    /// Current heal power of the Instance
    /// </summary>
    public abstract int CurrentHealPower { get; set; }
    /// <summary>
    /// Current transformation the Instance is in
    /// </summary>
    public abstract Transformations CurrentTransformation { get; set; }
    /// <summary>
    /// Equiped weapon of the Instance
    /// </summary>
    public WeaponInstance EquippedWeaponInstance;
    /// <summary>
    /// Gets the effective weapon damage of the current weapon (No buffs!)
    /// </summary>
    /// <remarks>Use this in GetEffectiveWeaponDamageAfterEffects()</remarks>
    /// <returns>The damage amount</returns>
    public int GetEffectiveWeaponDamage() => EquippedWeaponInstance?.GetEffectiveDamage() ?? 0;
    /// <summary>
    /// Gets the effictive crit chance of the current weapon (No Buffs!)
    /// </summary>
    /// <remarks>Use this in GetEffectiveCritChanceAfterEffects()</remarks>
    /// <returns>The crit chance (0 - 100)</returns>
    public int GetEffectiveCritChance() => EquippedWeaponInstance?.GetEffectiveCritChance() ?? 0;
    public abstract List<SkillSO> GetAllSkills();
    /// <summary>
    /// All effects the Instance has
    /// </summary>
    public List<Effect> ActiveEffects { get; private set; } = new List<Effect>();
    /// <summary>
    /// All active effect icons
    /// </summary>
    public List<GameObject> ActiveEffectIcons = new List<GameObject>();
    /// <summary>
    /// All upgrades the Instance has
    /// </summary>
    public List<Upgrade> ActiveUpgrades { get; private set; } = new List<Upgrade>();
    /// <summary>
    /// All Items the Instance has
    /// </summary>
    public List<Item> ActiveItems { get; private set; } = new List<Item>();
    /// <summary>
    /// Heal the Instance and activate all on-heal effects
    /// </summary>
    /// <param name="amount">The base heal</param>
    /// <remarks>Current heal power still gets added to param="amount"</remarks>
    /// <returns>The amount you healed</returns>
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

            BattleUIManager.Instance.AddLog($"{CharacterName} heals for {healAmount}!");
        }

        if ((effectiveHeal += CurrentHealth) > MaxHealth)
        {
            CheckForOverHealEffects();
        }

        return healAmount;
    }
    /// <summary>
    /// The instance takes damage, reduced by defence.
    /// </summary>
    /// <param name="amount">The amount of damage before defence</param>
    /// <param name="isSkill">wheter the dmg came from a skill</param>
    /// <param name="isEffect">Wheter the damage came from an effect</param>
    /// <returns>the result for a log / the damage the instanxce took</returns>
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
            HandleOnDamage(reducedDamage, isSkill);
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
    /// <summary>
    /// Handles the effect "Delfection"
    /// </summary>
    /// <returns>Wheter a deflect happend</returns>
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

        foreach (Item item in ActiveItems)
            switch (item.type)
            {
                case ItemType.StrikingFlower:
                    AddEffect(new Effect(EffectType.BloomBlossom, 2, false, 1));
                    break;
                default:
                    break;
            }

        return true; // een deflect is afgehandeld
    }
    /// <summary>
    /// Handles the "Sneaky" upgrade
    /// </summary>
    /// <returns>Wheter they dodged the attack</returns>
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
    /// <summary>
    /// Handles effects that happen on an "evasion" dodge
    /// </summary>
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
    /// <summary>
    /// Checks if the Instance has "evasion"
    /// </summary>
    /// <returns>If they have the effect</returns>
    private bool HasEvasionEffect()
    {
        return ActiveEffects.Exists(b => b.type == EffectType.Evasion);
    }
    /// <summary>
    /// Handles the "hidden" effect
    /// </summary>
    /// <returns>Wheter they have the effect</returns>
    private bool HandleHidden()
    {
        Effect hiddenEffect = ActiveEffects.Find(b => b.type == EffectType.Hidden);
        if (hiddenEffect != null)
        {
            return true;
        }
        return false;
    }
    /// <summary>
    /// Handles all "shield" like effects
    /// </summary>
    /// <param name="damage">the damage that the Instance would take</param>
    /// <returns>Wheter the shield blocked any damage</returns>
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

        Effect blessedShieldEffect = ActiveEffects.Find(b => b.type == EffectType.BlessedShield);
        if (blessedShieldEffect != null && blessedShieldEffect.intensity > 0)
        {
            blessedShieldEffect.intensity -= damage;

            if (blessedShieldEffect.intensity <= 0)
            {
                ActiveEffects.Remove(blessedShieldEffect);

                AddEffect(new Effect(EffectType.HealingStrenghten, 5, false, CurrentHealPower));
            }

            return true;
        }

        return false;
    }
    /// <summary>
    /// Handles effects that happen when a Combatant takes more then 0 dmg
    /// </summary>
    /// <param name="damage">the damage that is done</param>
    /// <param name="isSkill">if the damage came from a skill</param>
    /// <returns>Wheter something happened</returns>
    private void HandleOnDamage(int damage, bool isSkill)
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
        }
        Effect nutouCurse = ActiveEffects.Find(b => b.type == EffectType.NutouCurse);
        if (nutouCurse != null)
        {
            AddEffect(new Effect(EffectType.HealingWeaken, 1, true, nutouCurse.intensity));
        }

		Effect crimsonCurse = ActiveEffects.Find(c => c.type == EffectType.CrimsonCurse);
		if (crimsonCurse != null)
		{
			AddEffect(new Effect(EffectType.Burn, 1, true, crimsonCurse.intensity));
		}

        // Your Upgrades
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
        CombatantInstance player;
        CombatantInstance opponent;

        // Determine player & opponent
        if (this is DoobieInstance)
        {
            player = this;
            opponent = GameManager.Instance.currentVangurr;
        }
        else
        {
            player = this;
            opponent = GameManager.Instance.currentDoobie;
        }

        // Loop through opponent’s upgrades
        foreach (Upgrade opponentUpgrade in opponent.ActiveUpgrades)
        {
            switch (opponentUpgrade.type)
            {
                case UpgradeNames.BoneSnapper:
                    if (player.CurrentHealth != player.MaxHealth && player.CurrentTransformation == Transformations.SpiritForm)
                    {
                        player.AddEffect(new Effect(EffectType.Bleed, 2, true, opponentUpgrade.intensity));
                    }
                    break;
                default:
                    break;
            }
        }

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
        CheckForAttackEffects();

        switch (result)
        {
            case DamageResult.Deflected:
                return $"{CharacterName} strikes, but {target.CharacterName} deflects the blow with finesse!";
            case DamageResult.Hit:

                // Attacker's barrels explode onto the target
                ExplodeBarrels(this, target, false, false);

                // Defender's barrels explode back onto themselves
                ExplodeBarrels(target, this, true, false);

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
    /// <summary>
    /// Apply all on crit effects/upgrades and double the damage
    /// </summary>
    /// <param name="baseDamage">base damage</param>
    /// <returns>damage after all effects/upgrades</returns>
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
        modifiedDamage *= 2;
        return modifiedDamage;
    }
    /// <summary>
    /// Gets the effective weapon damage after effects
    /// </summary>
    /// <param name="baseDamage">base damage</param>
    /// <returns>modified damage</returns>
    public int GetEffectiveWeaponDamageAfterEffects(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        var effectsSnapshot = new List<Effect>(ActiveEffects);

        foreach (var effect in effectsSnapshot)
        {
            if (effect.type == EffectType.Rage)
            {
                modifiedDamage *= effect.intensity;
                ActiveEffects.Remove(effect);
            }
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
    /// <summary>
    /// Gets the effective weapon damage for the UI
    /// </summary>
    /// <param name="baseDamage">the base damage</param>
    /// <remarks>No effects happen</remarks>
    /// <returns>The modified damage amount</returns>
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
    /// <summary>
    /// Gets the effective Skill/Spell damage
    /// </summary>
    /// <param name="baseDamage">The base damage</param>
    /// <returns>The modified damage</returns>
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
    /// <summary>
    /// Gets the effective Crit chance from your weapon
    /// </summary>
    /// <param name="baseCrit">base crit chance</param>
    /// <returns>modified crit chance</returns>
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
    /// <summary>
    /// Gets the effective skill damage
    /// </summary>
    /// <remarks>Procs CheckForSkillOnUseEffects()</remarks>
    /// <remarks>Uses the GetEffectiveSkillDamageAfterEffects()</remarks>
    /// <param name="baseDmg">the base damage</param>
    /// <returns>modified damage</returns>
    public int GetEffectiveSkillDamage(int baseDmg)
    {
        int finalDmg;

        CheckForSpelllOnUseEffects();

        return finalDmg = GetEffectiveSkillDamageAfterEffects(baseDmg);
    }
    /// <summary>
    /// Gets the effective skill damage
    /// </summary>
    /// <remarks>Does NOT proc CheckForSkillOnUseEffects()</remarks>
    /// <remarks>Uses the GetEffectiveSkillDamageAfterEffects()</remarks>
    /// <param name="baseDmg">the base damage</param>
    /// <returns>modified damage</returns>
    public int GetEffectiveSkillDamageForUI(int baseDmg)
    {
        int finalDmg;

        return finalDmg = GetEffectiveSkillDamageAfterEffects(baseDmg);
    }
    /// <summary>
    /// Gets the effective heal power of the Instance
    /// </summary>
    /// <param name="baseHeal">base heal amount</param>
    /// <returns>modified heal amount</returns>
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
    /// <summary>
    /// Activates Effects/Upgrades that happen when you use a Spell-Style
    /// </summary>
    public void CheckForSpelllOnUseEffects()
    {
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
                case UpgradeNames.Shadowrend:
                    if (CurrentHealth == MaxHealth)
                    {
                        AddEffect(new Effect(EffectType.HealingStrenghten, 3, false, (upgrade.intensity * 3)));
                    }
                    break;
                default:
                    break;
            }
        }
        var effectsSnapshot = new List<Effect>(ActiveEffects);

        foreach (Item item in ActiveItems)
        {
            switch (item.type)
            {
                case ItemType.BleedingSpirit:
                    for (int i = 0; i < effectsSnapshot.Count; i++)
                    {
                        var effect = effectsSnapshot[i];
                        switch (effect.type)
                        {
                            case EffectType.Bleed:
                                var (result, damageDone) = TakeDamage(effect.intensity, false, true);
                                BattleUIManager.Instance.AddLog($"{CharacterName} takes {damageDone} bleed damage!");
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case ItemType.JarOfShadows:
                    AddEffect(new Effect(EffectType.Shadow, 5, false, 1));
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// Activates Effects/Upgrade that happen when you use your weapon
    /// </summary>
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
                        float chancePerIntensity = 0.05f;
                        float totalChance = upgrade.intensity * chancePerIntensity;

                        if (Random.value < totalChance)
                        {
                            AddEffect(new Effect(EffectType.Deflecion, 999, false, upgrade.intensity));
                        }
                        break;
                    case UpgradeNames.CalmRitual:
                        HealCombatant(upgrade.intensity);
                        break;
                    case UpgradeNames.HeartOfStillness:
                        if (this is DoobieInstance)
                        {
                            GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.NutouCurse, 1, true, upgrade.intensity));
                        }
                        else
                        {
                            GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.NutouCurse, 1, true, upgrade.intensity));
                        }
                        break;
                    case UpgradeNames.SereneCarapace:
                        AddEffect(new Effect(EffectType.ConvertOverheal, 5, false, upgrade.intensity));
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
                        var (result, damageDone) = TakeDamage(effect.intensity, false, true);
                        BattleUIManager.Instance.AddLog($"{CharacterName} takes {damageDone} bleed damage!");
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
                    case EffectType.HardHitter:
                        int bonusChance = 10 * effect.intensity;

                        if (this is DoobieInstance doobie)
                        {
                            int baseCrit = doobie.GetEffectiveCritChance();
                            int totalChance = doobie.GetEffectiveCritChanceAfterEffects(baseCrit + bonusChance);

                            totalChance -= 5;

                            bool stunEffect = Random.Range(0, 100) < totalChance;
                            if (stunEffect)
                            {
                                GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Stun, 1, true, effect.intensity));
                            }
                        }
                        else if (this is VangurrInstance vangurr)
                        {
                            int baseCrit = vangurr.GetEffectiveCritChance();
                            int totalChance = vangurr.GetEffectiveCritChanceAfterEffects(baseCrit + bonusChance);

                            totalChance -= 5;

                            bool stunEffect = Random.Range(0, 100) < totalChance;
                            if (stunEffect)
                            {
                                GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.Stun, 1, true, effect.intensity));
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
    /// <summary>
    /// Activates when you use a skill
    /// </summary>
    public void CheckForSkillOnUseEffects()
    {
        
    }
    /// <summary>
    /// Activates Effects/Upgrades that happen when you heal
    /// </summary>
    /// <remarks>does not include overheal</remarks>
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
                case UpgradeNames.VineLash:
                    if (this is DoobieInstance)
                    {
                        GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Vines, 2, true, upgrade.intensity));
                    }
                    else
                    {
                        GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.Vines, 2, true, upgrade.intensity));
                    }
                    break;
                case UpgradeNames.HealingFaith:
                    if (this is DoobieInstance doobie && doobie.CurrentGoddess == GoddessType.Elenara)
                    {
                        doobie.MainResource.Gain(upgrade.intensity);
                        BattleUIManager.Instance.AddLog($"{CharacterName} has gained 2 Faith!");
                    }
                    break;
                case UpgradeNames.IronBreath:
                    if (CurrentHealth >= (MaxHealth / 2))
                    {
                        AddEffect(new Effect(EffectType.HealingStrenghten, 3, false, upgrade.intensity));
                    }
                    break;
                default:
                    break;
            }
        }
        var effectsSnapshot = new List<Effect>(ActiveEffects);

        foreach (var effect in effectsSnapshot)
        {
            switch (effect.type)
            {
                case EffectType.Vines:
                    BattleUIManager.Instance.AddLog($"{CharacterName} {effect.intensity} vines activate!");
                    TakeDamage(effect.intensity);
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// Activates Effects/Upgrades that happen when you overheal
    /// </summary>
    public void CheckForOverHealEffects()
    {
        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.OverflowingGrace:
                    AddEffect(new Effect(EffectType.Regeneration, 1, false, upgrade.intensity));
                    break;
                default:
                    break;
            }
        }

        var effectsSnapshot = new List<Effect>(ActiveEffects);

        foreach (var effect in effectsSnapshot)
        {
            switch (effect.type)
            {
                case EffectType.ConvertOverheal:
                    AddEffect(new Effect(EffectType.Shield, 10, false, effect.intensity));
                    break;
            }
        }
    }
    /// <summary>
    /// Activates Effects/upgrades that happen when you use a basic attack
    /// </summary>
    /// <param name="target"></param>
    protected void ApplyEffectsOnBasicAttack(CombatantInstance target)
    {
        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.Firebrand:
                    target.AddEffect(new Effect(EffectType.Burn, 3, true, upgrade.intensity));
                    break;
                case UpgradeNames.WrathboundOath:
                    if (this is DoobieInstance doobie && doobie.MainResource.Current >= 5)
                    {
                        AddEffect(new Effect(EffectType.WeaponStrenghten, 3, false, upgrade.intensity));
                        AddEffect(new Effect(EffectType.CriticalEye, 3, false, upgrade.intensity));
                    }
                    break;
            }
        }

        var effectsSnapshot = new List<Effect>(ActiveEffects);

        foreach (var effect in effectsSnapshot)
        {
            switch (effect.type)
            {
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// Activates Effects/upgrades that happen when you get a effect
    /// </summary>
    /// <param name="newEffect">the effect you get</param>
    private void AddEffectUpgradesCheck(Effect newEffect)
    {
        CombatantInstance player;
        CombatantInstance opponent;

        // Determine player & opponent
        if (this is DoobieInstance)
        {
            player = this;
            opponent = GameManager.Instance.currentVangurr;
        }
        else
        {
            player = this;
            opponent = GameManager.Instance.currentDoobie;
        }

        if (newEffect.isDebuff)
        {
            Upgrade cursedFaithUpgrade = GameManager.Instance.currentDoobie.ActiveUpgrades.Find(c => c.type == UpgradeNames.CursedFaith);
            if (cursedFaithUpgrade != null)
            {
                if (this is DoobieInstance doobie && doobie.CurrentGoddess == GoddessType.Velithra)
                {
                    GameManager.Instance.currentDoobie.MainResource.Gain(cursedFaithUpgrade.intensity);
                    BattleUIManager.Instance.AddLog($"{CharacterName} has gained 2 Faith!");
                }
            }
        }

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

        if (this is DoobieInstance)
        {
            Upgrade maskOfMidnight = GameManager.Instance.currentVangurr.ActiveUpgrades.Find(m => m.type == UpgradeNames.MaskOfMidnight);
            if (maskOfMidnight != null)
            {
                AddEffect(new Effect(EffectType.Holy, 2, true, maskOfMidnight.intensity));
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

        if (newEffect.type == EffectType.Hidden)
        {
            Upgrade howlingRushUpgrade = ActiveUpgrades.Find(b => b.type == UpgradeNames.HowlingRush);
            if (howlingRushUpgrade != null)
            {
                AddEffect(new Effect(EffectType.Regeneration, 1, false, (howlingRushUpgrade.intensity * 5)));
            }
        }

        if (newEffect.type == EffectType.Bleed)
        {
            Upgrade soulflareUpgrade = opponent.ActiveUpgrades.Find(b => b.type == UpgradeNames.SoulflareEdge);
            if (soulflareUpgrade != null)
            {
                opponent.HealCombatant(soulflareUpgrade.intensity);
            }
        }

        if (newEffect.type == EffectType.WeaponStrenghten)
        {
            Upgrade furyStrikeUpgrade = player.ActiveUpgrades.Find(f => f.type == UpgradeNames.FuryStrike);
            if (furyStrikeUpgrade != null)
            {
                opponent.TakeDamage(furyStrikeUpgrade.intensity);
            }
        }
    }
    /// <summary>
    /// Sets the currentHealth to 0
    /// </summary>
    public void KillInstance()
    {
        CurrentHealth = 0;
    }
    /// <summary>
    /// Effects/Upgrades that happen when you use a skill/basic attack
    /// </summary>
    public void CheckForAttackEffects()
    {
        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.BattleFaith:
                    if (this is DoobieInstance doobie && doobie.CurrentGoddess == GoddessType.Kaelyth)
                    {
                        doobie.MainResource.Gain(upgrade.intensity);
                        BattleUIManager.Instance.AddLog($"{CharacterName} has gained 2 Faith!");
                    }
                    break;
                case UpgradeNames.SpearOfRadiance:
                    bool hasDebuff = false;
                    while (hasDebuff == false)
                    {
                        foreach (var effect in ActiveEffects)
                        {
                            if (effect.isDebuff == true)
                            {
                                hasDebuff = true;
                                return;
                            }
                        }
                    }
                    if (hasDebuff)
                    {
                        if (this is DoobieInstance)
                        {
                            GameManager.Instance.currentVangurr.TakeDamage((upgrade.intensity * 3));
                        }
                        else
                        {
                            GameManager.Instance.currentDoobie.TakeDamage((upgrade.intensity * 3));
                        }
                    }
                    break;
                case UpgradeNames.EchoExplosion:
                    if (this is DoobieInstance)
                    {
                        GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.TimedBomb, 5, true, upgrade.intensity));
                    }
                    else
                    {
                        GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.TimedBomb, 5, true, upgrade.intensity));
                    }
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// Adds an effect into the ActiveEffects on the Instance
    /// </summary>
    /// <param name="newEffect">the effect you gain</param>
    public void AddEffect(Effect newEffect)
    {
        BeforeEffectGain(newEffect);

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
    /// <summary>
    /// Adds an upgrade into the ActiveUpgrades on the Instance
    /// </summary>
    /// <param name="newUpgrade"></param>
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
    public void AddItem(Item newItem)
    {
        Item existing = ActiveItems.Find(i => i.type == newItem.type);
        if (existing != null)
        {
            Debug.Log($"Already owns item: {newItem.itemName}");
            return;
        }

        ActiveItems.Add(newItem);
        Debug.Log($"Added active item: {newItem.itemName}");
    }

    /// <summary>
    /// Activates Effects/Upgrades the happen before you gain an effect
    /// </summary>
    /// <param name="newEffect">the effect you would gain</param>
    public void BeforeEffectGain(Effect newEffect)
    {
        Effect holyEffect = ActiveEffects.Find(h => h.type == EffectType.Holy);
        if (holyEffect != null)
        {
            int holyDamage = 0;
            for (int i = 0;  i < holyEffect.intensity; i++)
            {
                holyDamage += 1;
            }

            TakeDamage(holyDamage);
            BattleUIManager.Instance.AddLog($"{CharacterName} takes damage beacause they gained a debuff while they have Holy!");
        }
    }
    /// <summary>
    /// Reduce the turn counter on all ActiveEffects by 1
    /// </summary>
    public void TickEffects()
    {
        for (int i = ActiveEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffects[i].duration--;

            if (ActiveEffects[i].duration <= 0)
            {
                var expiredEffect = ActiveEffects[i];

                switch (expiredEffect.type)
                {
                    case EffectType.TargetLocked:
                        ActivateTargetLocked(expiredEffect);
                        break;

                    case EffectType.TimedBomb:
                        ActivateTimedBomb(expiredEffect);
                        break;
                }

                ActiveEffects.RemoveAt(i);
            }
        }

        BattleUIManager.Instance.UpdateEffectsUI(this,
            this is DoobieInstance
                ? BattleUIManager.Instance.DoobieEffectsContainer
                : BattleUIManager.Instance.VangurrEffectsContainer
        );
    }
    private void ActivateTargetLocked(Effect expired)
    {
        var (result, damageDone) = TakeDamage(expired.intensity);
        BattleUIManager.Instance.AddLog($"Target Locked activates! dealing {damageDone} damage!");

        // --- Target Garden synergy ---
        if (this is DoobieInstance)
        {
            var opponentUpgrade = GameManager.Instance.currentVangurr.ActiveUpgrades.Find(u => u.type == UpgradeNames.TargetGarden);
            if (opponentUpgrade != null)
            {
                GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Regeneration, 2, true, opponentUpgrade.intensity));
            }
        }
        else
        {
            var opponentUpgrade = GameManager.Instance.currentDoobie.ActiveUpgrades.Find(u => u.type == UpgradeNames.TargetGarden);
            if (opponentUpgrade != null)
            {
                GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.Regeneration, 2, true, opponentUpgrade.intensity));
            }
        }
        if (this is DoobieInstance)
        {
            var opponentTargetScoped = GameManager.Instance.currentVangurr.ActiveItems.Find(u => u.type == ItemType.TargetScoped);
            if (opponentTargetScoped != null)
            {
                GameManager.Instance.currentVangurr.AddEffect(
                    new Effect(EffectType.TargetLocked, expired.duration + 1, true, expired.intensity)
                );
                BattleUIManager.Instance.AddLog($"TargetScoped");
            }
        }
        else
        {
            var opponentTargetScoped = GameManager.Instance.currentDoobie.ActiveItems.Find(u => u.type == ItemType.TargetScoped);
            if (opponentTargetScoped != null)
            {
                GameManager.Instance.currentVangurr.AddEffect(
                    new Effect(EffectType.TargetLocked, expired.duration + 1, true, expired.intensity)
                );
                BattleUIManager.Instance.AddLog($"TargetScoped");
            }
        }
    }
    private void ActivateTimedBomb(Effect expired)
    {
        int baseDmg = 0;

        if (this is DoobieInstance)
        {
            baseDmg += GameManager.Instance.currentVangurr.GetEffectiveSkillDamage(GameManager.Instance.currentVangurr.CurrentSkillDmg);
        }
        else
        {
            baseDmg += GameManager.Instance.currentDoobie.GetEffectiveSkillDamage(GameManager.Instance.currentDoobie.CurrentSkillDmg);
        }

        baseDmg *= expired.intensity;

        TakeDamage(baseDmg);
        BattleUIManager.Instance.AddLog($"{CharacterName}'s Timed Bomb explodes for {baseDmg} damage!");
    }


    /// <summary>
    /// Gets the effective defence of the Instance
    /// </summary>
    /// <returns>the modified defence</returns>
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
        switch (CurrentTransformation)
        {
            case Transformations.SpiritForm:
                defence *= 0.5f;
                break;
        }

        return defence;
    }
    /// <summary>
    /// Explode all the barrels on the field
    /// </summary>
    /// <param name="owner">Owner of the barrel</param>
    /// <param name="opponent">The enemy of the owner</param>
    /// <param name="ownerIsBeingAttacked">wheter the owner of the barrels if being attacked</param>
    /// <param name="cameFromPistolShot">wheter the call came from a pistol shot</param>
    public void ExplodeBarrels(CombatantInstance owner, CombatantInstance opponent, bool ownerIsBeingAttacked, bool cameFromPistolShot)
    {
        List<Effect> effectsSnapshot = new List<Effect>(owner.ActiveEffects);

        foreach (Effect effect in effectsSnapshot)
        {
            if (effect.type == EffectType.Barrel)
            {
                // If owner is being attacked, they get hurt by their own barrels
                CombatantInstance victim = ownerIsBeingAttacked ? owner : opponent;

                bool isCrit = Random.Range(0, 101) < GetEffectiveCritChanceAfterEffects(GetEffectiveCritChance());

                int damageBeforeCrit = effect.intensity;

                if (!ownerIsBeingAttacked)
                {
                    damageBeforeCrit += owner.GetEffectiveWeaponDamageAfterEffects(owner.GetEffectiveWeaponDamage());
                }
                else
                {
                    damageBeforeCrit += opponent.GetEffectiveWeaponDamageAfterEffects(opponent.GetEffectiveWeaponDamage());
                }

                if (!ownerIsBeingAttacked)
                {
                    Upgrade fiercePowder = ActiveUpgrades.Find(b => b.type == UpgradeNames.FiercePowder);
                    if (fiercePowder != null)
                    {
                        damageBeforeCrit += fiercePowder.intensity;
                    }
                }
                else
                {
                    Upgrade paddedBarrels = ActiveUpgrades.Find(b => b.type == UpgradeNames.PaddedBarrels);
                    if (paddedBarrels != null)
                    {
                        damageBeforeCrit -= paddedBarrels.intensity;
                    }
                }

                if (isCrit)
                {
                    int damageAfterCrit = ApplyCriticalHitEffects(damageBeforeCrit);

                    Upgrade criticalBarrelUpgrade = ActiveUpgrades.Find(b => b.type == UpgradeNames.CriticalBarrels);
                    if (criticalBarrelUpgrade != null && !ownerIsBeingAttacked)
                    {
                        AddEffect(new Effect(EffectType.CriticalEye, 3, false, criticalBarrelUpgrade.intensity));
                        AddEffect(new Effect(EffectType.Barrel, 100, false, criticalBarrelUpgrade.intensity));
                    }

                    damageBeforeCrit = damageAfterCrit;
                }

                if (cameFromPistolShot)
                {
                    damageBeforeCrit *= 2;
                    if (!ownerIsBeingAttacked)
                    for (int i = 0; i < effect.intensity; i++)
                    {
                        GameManager.Instance.ChangeSploont(10, true);
                    }
                    BattleUIManager.Instance.AddLog($"{CharacterName} Gains {10 * effect.intensity} sploont!");
                }

                int finalDamage = damageBeforeCrit;

                var (result, damageDone) = victim.TakeDamage(finalDamage, false, true);

                if (ownerIsBeingAttacked)
                    if (isCrit)
                    {
                        BattleUIManager.Instance.AddLog($"{owner.CharacterName}'s barrels explode backfiring \n dealing {damageDone} CRITICAl damage to themselves!\n");
                    }
                    else
                    {
                        BattleUIManager.Instance.AddLog($"{owner.CharacterName}'s barrels explode backfiring \n dealing {damageDone} damage to themselves!\n");
                    }
                else
                {
                    if (isCrit)
                    {

                        BattleUIManager.Instance.AddLog($"{owner.CharacterName}'s barrels explode \n blasting {opponent.CharacterName} for {damageDone} CRITICAL damage!");
                    }
                    else
                    {
                        BattleUIManager.Instance.AddLog($"{owner.CharacterName}'s barrels explode \n blasting {opponent.CharacterName} for {damageDone} damage!");
                    }
                }

                owner.ActiveEffects.Remove(effect);

                if (ownerIsBeingAttacked)
                {
                    bool hasDualBarrels = owner.ActiveItems.Any(i => i.type == ItemType.DualBarrels);
                    if (hasDualBarrels)
                    {
                        int barrelSpawnAmount = effect.intensity;
                        owner.AddEffect(new Effect(EffectType.Barrel, 100, false, barrelSpawnAmount));
                        BattleUIManager.Instance.AddLog($"{owner.CharacterName}'s Dual Barrels rebuild themselves after the explosion!");
                    }
                }
            }
        }
    }
    public void OnBurnDamage(int damage)
    {
        CombatantInstance opponent = GetOpponent();

        // --- Upgrade reactions ---
        foreach (var upgrade in opponent.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.WalkThePlank:
                    var barrelEffect = new Effect(EffectType.Barrel, 100, false, upgrade.intensity);
                    opponent.AddEffect(barrelEffect);
                    BattleUIManager.Instance.AddLog($"{CharacterName}'s Walk The Plank creates a Barrel!");
                    break;
                default:
                    break;
            }
        }

        foreach (Item item in opponent.ActiveItems)
        {
            switch (item.type)
            {
                case ItemType.Fuel:
                    // Double the intensity of all burn effects on the opponent
                    var burnEffects = opponent.ActiveEffects
                                        .Where(e => e.type == EffectType.Burn)
                                        .ToList();
                    foreach (var burn in burnEffects)
                    {
                        burn.intensity *= 2;
                    }
                    BattleUIManager.Instance.AddLog($"{CharacterName}'s Fuel item doubles opponent's burn intensity!");
                    break;
                default:
                    break;
            }
        }
    }

    // --- Helper: get the current opponent ---
    private CombatantInstance GetOpponent()
    {
        if (this is DoobieInstance)
            return GameManager.Instance.currentVangurr;
        else
            return GameManager.Instance.currentDoobie;
    }


    /// <summary>
    /// Sets the transformation of the Instance
    /// </summary>
    /// <param name="transformation">The transformation the Instance becomes</param>
    public void SetTransformation(Transformations transformation)
    {
        CurrentTransformation = transformation;
        Debug.Log($"Current Transformation: {CurrentTransformation} / Chosen: {transformation}");

        BattleUIManager.Instance.AddLog($"{CharacterName} has transformed!");

        BattleUIManager.Instance.CombatantTransformation(this, transformation);

        OnTransformation();

        BattleUIManager.Instance.RefreshSkillButtons(GetAllSkills());
    }
    /// <summary>
    /// Activates Effects/Upgrades that happen when you transform
    /// </summary>
    void OnTransformation()
    {
        foreach (var Upgrade in ActiveUpgrades)
        {
            switch (Upgrade.type)
            {
                case UpgradeNames.BloodiedMomentum:
                    if (this is DoobieInstance doobie)
                    {
                        GameManager.Instance.currentVangurr.AddEffect(new Effect(EffectType.Bleed, 3, true, (Upgrade.intensity * 3)));
                    }
                    else
                    {
                        GameManager.Instance.currentDoobie.AddEffect(new Effect(EffectType.Bleed, 3, true, (Upgrade.intensity * 3)));
                    }
                    break;
                default:
                    break;
            }
        }
    }
    /// <summary>
    /// Activate the animation of the weapon
    /// </summary>
    /// <param name="animationPrefab">The animation prefab</param>
    public void PlayAttackAnimation(GameObject animationPrefab)
    {
        if (animationPrefab == null || animationAnchor == null)
            return;

        // Spawn it as a child of the anchor, matching rotation & prefab scale
        GameObject spawned = GameObject.Instantiate(animationPrefab, animationAnchor.position, animationAnchor.rotation, animationAnchor);
        spawned.transform.localScale = animationPrefab.transform.localScale;

        // Handle all renderers, not just the main ParticleSystem
        foreach (var renderer in spawned.GetComponentsInChildren<Renderer>(true))
        {
            renderer.sortingLayerName = "VFXForeground";
            renderer.sortingOrder = 100;
        }

        // Optional safety: make sure it’s active and visible
        spawned.SetActive(true);

        // Auto-destroy after the longest particle or animation ends
        float lifeTime = 2f;
        var ps = spawned.GetComponent<ParticleSystem>();
        if (ps != null)
            lifeTime = ps.main.duration + ps.main.startLifetime.constantMax;

        GameObject.Destroy(spawned, lifeTime);
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
