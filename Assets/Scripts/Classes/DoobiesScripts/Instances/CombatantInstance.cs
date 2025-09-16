using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public abstract class CombatantInstance
{

    public Transform animationAnchor;

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
    public List<Buff> ActiveBuffs { get; private set; } = new List<Buff>();
    public List<GameObject> ActiveBuffIcons = new List<GameObject>();

    public List<Upgrade> ActiveUpgrades { get; private set; } = new List<Upgrade>();

    public int HealCombatant(int amount)
    {
        int effectiveHeal = GetEffectiveHealPower(amount);

        int healAmount = Mathf.Min(effectiveHeal, MaxHealth - CurrentHealth);
        CurrentHealth += healAmount;
        return healAmount;
    }
    /// <summary>
    /// The instance takes damage, reduced by defence.
    /// </summary>
    /// <param name="amount">The amount of damage before defence</param>
    /// <param name="isSkill">wheter the dmg came from a skill</param>
    /// <returns>The actual damage</returns>
    public virtual (DamageResult result, int actualDamage) TakeDamage(int amount, bool isSkill = false)
    {
        Debug.Log("Taking base damage: " + amount);

        if (HandleDeflection())
            return (DamageResult.Deflected, 0);

        // alleen melee / weapon attacks (niet skills) mogen dodges via Sneaky/Evasion doen
        if (!isSkill)
        {
            // 1) Als er al een Evasion-buff is, dit is een "Evasion dodge" -> trigger follow-ups
            if (HasEvasionBuff())
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

        HandeCurseEffect(reducedDamage);

        CurrentHealth = Mathf.Max(CurrentHealth - reducedDamage, 0);

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
        var deflectBuffs = ActiveBuffs.FindAll(b => b.type == BuffType.Deflecion);

        if (deflectBuffs.Count == 0)
            return false; // niks om te doen

        // Harden buff geven als er een sterke deflect was
        if (deflectBuffs.Any(b => b.intensity >= 10))
        {
            AddBuff(new Buff(BuffType.Harden, 3, false, 3));
        }

        // Alle deflects weghalen
        ActiveBuffs.RemoveAll(b => b.type == BuffType.Deflecion);

        // Check voor BloomBlossom ? herplaats deflection
        Buff bloomBlossomBuff = ActiveBuffs.Find(b => b.type == BuffType.BloomBlossom);
        if (bloomBlossomBuff != null)
        {
            AddBuff(new Buff(BuffType.Deflecion, 999, false, 10));

            ActiveBuffs.RemoveAll(b => b.type == BuffType.BloomBlossom);

            Upgrade ultimateBloomUpgrade = ActiveUpgrades.Find(b => b.type == UpgradeNames.UltimateBloom);
            if (ultimateBloomUpgrade != null)
            {
                for (int i = 0; i < ultimateBloomUpgrade.intensity; i++)
                {
                    AddBuff(new Buff(BuffType.WeaponStrenghten, 1, false, 1));
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
            AddBuff(new Buff(BuffType.Evasion, sneakyStacks, false, sneakyStacks));

            return true;
        }

        return false;
    }
    private void HandleDodgeEffects()
    {
        var evasionBuff = ActiveBuffs.Find(b => b.type == BuffType.Evasion);
        if (evasionBuff == null) return;

        // Voor elke stack (intensity) van Evasion: geef follow-up buffs
        int stacks = Mathf.Max(1, evasionBuff.intensity); // defensive
        AddBuff(new Buff(BuffType.WeaponStrenghten, 2, false, stacks));
        AddBuff(new Buff(BuffType.CriticalEye, 2, false, stacks));

        evasionBuff.duration--;
        if (evasionBuff.duration <= 0)
            ActiveBuffs.Remove(evasionBuff);
    }

    private bool HasEvasionBuff()
    {
        return ActiveBuffs.Exists(b => b.type == BuffType.Evasion);
    }
    private bool HandleShield(int damage)
    {
        Buff shieldBuff = ActiveBuffs.Find(b => b.type == BuffType.Shield);
        if (shieldBuff != null && shieldBuff.intensity > 0)
        {
            shieldBuff.intensity -= damage;

            if (shieldBuff.intensity <= 0)
            {
                ActiveBuffs.Remove(shieldBuff);
            }

            return true;
        }

        return false;
    }
    private bool HandeCurseEffect(int damage)
    {
        Buff vampireCurse = ActiveBuffs.Find(b => b.type == BuffType.VampireCurse);
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

        Buff BlindDeBuff = ActiveBuffs.Find(b => b.type == BuffType.Blind);
        if (Random.value < EquippedWeaponInstance.MissChance || BlindDeBuff != null)
        {
            return $"{CharacterName} swings at {target.CharacterName}, but misses!";
        }

        float multiplier = Random.Range(0.5f, 1.5f);

        int baseDamage = Mathf.RoundToInt(attack * multiplier);

        // Apply any attack-affecting buffs
        int baseDamageAfterBuffs = GetEffectiveWeaponDamageAfterBuffs(baseDamage);

        bool isCrit = Random.Range(0, 100) < GetEffectiveCritChance();
        int finalDamage = isCrit ? baseDamageAfterBuffs * 2 : baseDamageAfterBuffs;

        // Activate Effects
        if (EquippedWeaponInstance.Animation != null)
        {
            target.PlayAttackAnimation(EquippedWeaponInstance.Animation);
        }

        var (result, actualDamage) = target.TakeDamage(finalDamage);

        // Apply all upgrade effects
        ApplyUpgradeEffectsOnBasicAttack(target);

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
    public int GetEffectiveWeaponDamageAfterBuffs(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var buff in ActiveBuffs)
        {
            switch (buff.type)
            {
                case BuffType.WeaponWeaken:
                    for (int i = 0; i < buff.intensity; i++)
                        modifiedDamage = Mathf.FloorToInt(modifiedDamage * 0.8f);
                    break;
                case BuffType.WeaponStrenghten:
                    for (int i = 0; i < buff.intensity; i++)
                        modifiedDamage = Mathf.CeilToInt(modifiedDamage * 1.2f);
                    break;
            }
        }

        CheckForWeaponOnUseEffects();

        return Mathf.Max(modifiedDamage, 0);
    }
    public int GetEffectiveWeaponDamageAfterBuffsForUI(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var buff in ActiveBuffs)
        {
            switch (buff.type)
            {
                case BuffType.WeaponWeaken:
                    for (int i = 0; i < buff.intensity; i++)
                        modifiedDamage = Mathf.FloorToInt(modifiedDamage * 0.8f);
                    break;
                case BuffType.WeaponStrenghten:
                    for (int i = 0; i < buff.intensity; i++)
                        modifiedDamage = Mathf.CeilToInt(modifiedDamage * 1.2f);
                    break;
            }
        }

        return Mathf.Max(modifiedDamage, 0);
    }
    public int GetEffectiveSkillDamageAfterBuffs(int baseDamage)
    {
        int modifiedDamage = baseDamage;

        foreach (var buff in ActiveBuffs)
        {
            switch (buff.type)
            {
                case BuffType.SpellWeaken:
                    for (int i = 0; i < buff.intensity; i++)
                        modifiedDamage = Mathf.FloorToInt(modifiedDamage * 0.8f);
                    break;
                case BuffType.SpellStrenghten:
                    for (int i = 0; i < buff.intensity; i++)
                        modifiedDamage = Mathf.CeilToInt(modifiedDamage * 1.2f);
                    break;
            }
        }
        return Mathf.Max(modifiedDamage, 0);
    }

    public int GetEffectiveSkillDamage(int baseDmg)
    {
        int finalDmg;

        CheckForSkillOnUseEffects();

        return finalDmg = GetEffectiveSkillDamageAfterBuffs(baseDmg);
    }
    public int GetEffectiveSkillDamageForUI(int baseDmg)
    {
        int finalDmg;

        return finalDmg = GetEffectiveSkillDamageAfterBuffs(baseDmg);
    }
    public int GetEffectiveHealPower(int baseHeal)
    {
        int modifiedHeal = baseHeal;
        foreach (var buff in ActiveBuffs)
        {
            switch (buff.type)
            {
                default:
                    break;
            }
        }

        return Mathf.Max(modifiedHeal, 0);
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
                        AddBuff(new Buff(BuffType.WeaponStrenghten, 1, false, upgrade.intensity));
                        break;
                    case UpgradeNames.BloodyWeapon:
                        if (this is DoobieInstance)
                        {
                            GameManager.Instance.currentVangurr.AddBuff(new Buff(BuffType.Bleed, 3, true, upgrade.intensity));
                        }
                        else
                        {
                            GameManager.Instance.currentDoobie.AddBuff(new Buff(BuffType.Bleed, 3, true, upgrade.intensity));
                        }
                        break;
                    case UpgradeNames.ViolentAttacks:
                        if (this is DoobieInstance)
                        {
                            GameManager.Instance.currentDoobie.AddBuff(new Buff(BuffType.Bleed, 2, true, 2));

                            GameManager.Instance.currentDoobie.AddBuff(new Buff(BuffType.WeaponStrenghten, 3, true, upgrade.intensity));
                        }
                        else
                        {
                            GameManager.Instance.currentVangurr.AddBuff(new Buff(BuffType.Bleed, 2, true, 2));

                            GameManager.Instance.currentVangurr.AddBuff(new Buff(BuffType.WeaponStrenghten, 3, true, upgrade.intensity));
                        }
                        break;
                    case UpgradeNames.OffensiveFlow:
                        // Each intensity adds 5% chance to gain 1 Deflection
                        float chancePerIntensity = 0.05f;
                        float totalChance = upgrade.intensity * chancePerIntensity;

                        if (Random.value < totalChance)
                        {
                            AddBuff(new Buff(BuffType.Deflecion, 999, false, upgrade.intensity));
                        }
                        break;
                }
            }
        }
        if (ActiveBuffs != null)
        {
            foreach (var buff in ActiveBuffs)
            {
                switch (buff.type)
                {
                    case BuffType.Bleed:
                        for (int i = 0; i < buff.intensity; i++)
                        {
                            TakeDamage(1);
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }

    protected void ApplyUpgradeEffectsOnBasicAttack(CombatantInstance target)
    {
        if (ActiveUpgrades == null) return;

        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.Firebrand:
                    target.AddBuff(new Buff(BuffType.Burn, 3, true, upgrade.intensity)); 
                    break;
            }
        }
    }
    private void AddBuffUpgradesCheck(Buff newBuff)
    {
        if (newBuff.type == BuffType.Deflecion)
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
    }

    public void AddBuff(Buff newBuff)
    {
        Buff existing = ActiveBuffs.Find(b => b.type == newBuff.type);

        if (existing != null)
        {
            existing.duration += newBuff.duration;
            existing.intensity += newBuff.intensity;

            // Play stacking effect
            if (existing.iconInstance != null)
                existing.iconInstance.PlayEffect();
        }
        else
        {
            ActiveBuffs.Add(newBuff);
        }

        AddBuffUpgradesCheck(newBuff);

        Transform buffContainer = this is DoobieInstance
            ? BattleUIManager.Instance.DoobieBuffsContainer
            : BattleUIManager.Instance.VangurrBuffsContainer;

        BattleUIManager.Instance.UpdateBuffsUI(this, buffContainer);

        // --- Add quick combat log entry ---
        string buffName = newBuff.iconGO != null ? newBuff.iconGO.name : newBuff.type.ToString();
        string logMessage = $"{CharacterName} gains {newBuff.intensity} \"{buffName}\"!";
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

    public void TickBuffs()
    {
        for (int i = ActiveBuffs.Count - 1; i >= 0; i--)
        {
            ActiveBuffs[i].duration--;
            if (ActiveBuffs[i].duration <= 0)
                ActiveBuffs.RemoveAt(i);
        }
    }
    public float GetEffectiveDefence()
    {
        float defence = CurrentDefence;

        foreach (var Buffs in ActiveBuffs)
        {
            if (Buffs.type == BuffType.DefenceDown)
            {
                int i = Buffs.intensity;
                while (i > 1)
                {
                    defence *= 0.8f;
                    i--;
                }
            }
            else if (Buffs.type == BuffType.Harden)
            {
                int i = Buffs.intensity;
                while (i > 1)
                {
                    defence *= 1.2f;
                    i--;
                }
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

