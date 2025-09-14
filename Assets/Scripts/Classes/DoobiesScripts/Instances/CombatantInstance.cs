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

    public WeaponInstance EquippedWeaponInstance;

    public int GetEffectiveWeaponDamage() => EquippedWeaponInstance?.GetEffectiveDamage() ?? 0;
    public int GetEffectiveCritChance() => EquippedWeaponInstance?.GetEffectiveCritChance() ?? 0;

    public abstract List<SkillSO> GetAllSkills();
    public List<Buff> ActiveBuffs { get; private set; } = new List<Buff>();
    public List<GameObject> ActiveBuffIcons = new List<GameObject>();

    public List<Upgrade> ActiveUpgrades { get; private set; } = new List<Upgrade>();


    /// <summary>
    /// The instance takes damage, reduced by defence.
    /// </summary>
    /// <param name="amount">The amount of damage before defence</param>
    /// <param name="isSkill">wheter the dmg came from a skill</param>
    /// <returns>The actual damage</returns>
    public virtual (DamageResult result, int actualDamage) TakeDamage(int amount, bool isSkill = false)
    {
        if (HandleDeflection())
            return (DamageResult.Deflected, 0);

        if (HandleSneaky() || HasEvasionBuff())
        {
            HandleDodgeEffects(); // Apply follow-up buffs
            return (DamageResult.Dodged, 0);
        }

        float defence = GetEffectiveDefence();
        int reducedDamage = Mathf.CeilToInt(amount / defence);

        CurrentHealth = Mathf.Max(CurrentHealth - reducedDamage, 0);
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
        // Find all Sneaky upgrades
        int sneakyStacks = ActiveUpgrades.Count(u => u.type == UpgradeNames.Sneaky);
        if (sneakyStacks == 0)
            return false; // No Sneaky upgrades, no extra dodge

        // Each stack gives 25% dodge
        float dodgeChance = sneakyStacks * 0.25f;

        // Roll to see if dodge triggers
        if (Random.value < dodgeChance)
        {
            // Apply Evasion buff
            AddBuff(new Buff(BuffType.Evasion, duration: 1, isDebuff: false, intensity: 1));
            return true; // Dodge successful
        }

        return false; // Dodge failed
    }
    private void HandleDodgeEffects()
    {
        // Check if the combatant has an active Evasion buff
        var evasionBuff = ActiveBuffs.Find(b => b.type == BuffType.Evasion);
        if (evasionBuff != null)
        {
            // For dodges with Evasion, add WeaponStrengthen and TargetLocked buffs
            for (int i = 0; i < evasionBuff.intensity; i++)
            {
                AddBuff(new Buff(BuffType.WeaponStrenghten, duration: 2, isDebuff: false, intensity: 1));
                AddBuff(new Buff(BuffType.CriticalEye, duration: 2, isDebuff: false, intensity: 1));
            }

            // Optional: remove or tick down the Evasion buff after triggering
            evasionBuff.duration--;
            if (evasionBuff.duration <= 0)
                ActiveBuffs.Remove(evasionBuff);
        }
    }
    private bool HasEvasionBuff()
    {
        return ActiveBuffs.Exists(b => b.type == BuffType.Evasion);
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

        var attack = EquippedWeaponInstance.BasicAttack;

        Buff BlindDeBuff = ActiveBuffs.Find(b => b.type == BuffType.Blind);
        if (Random.value < EquippedWeaponInstance.MissChance || BlindDeBuff != null)
        {
            return $"{CharacterName} swings at {target.CharacterName}, but misses!";
        }

        float multiplier = Random.Range(0.5f, 1.5f);

        int baseDamage = Mathf.RoundToInt(attack.damage * multiplier);

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

        CheckForWeaponOnUseUpgrades();

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

        CheckForSkillOnUseUpgrades();

        return finalDmg = GetEffectiveSkillDamageAfterBuffs(baseDmg);
    }

    public void CheckForSkillOnUseUpgrades()
    {
        if (ActiveUpgrades == null) return;
        foreach (var upgrade in ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.SpellSlinger:
                    for (int i = 0; i < upgrade.intensity; i++)
                    {
                        if (this is DoobieInstance)
                        {
                            GameManager.Instance.currentVangurr.TakeDamage(1);
                        }
                        else
                        {
                            GameManager.Instance.currentDoobie.TakeDamage(1);
                        }
                    }
                    break;
            }
        }
    }
    public void CheckForWeaponOnUseUpgrades()
    {
        if (ActiveUpgrades != null)
        {
            foreach (var upgrade in ActiveUpgrades)
            {
                switch (upgrade.type)
                {
                    case UpgradeNames.WeaponMastery:
                        for (int i = 0; i < upgrade.intensity; i++)
                        {
                            AddBuff(new Buff(BuffType.WeaponStrenghten, 1, false, 1));
                        }
                        break;
                    case UpgradeNames.BloodyWeapon:
                        for (int i = 0; i < upgrade.intensity; i++)
                        {
                            if (this is DoobieInstance)
                            {
                                GameManager.Instance.currentVangurr.AddBuff(new Buff(BuffType.Bleed, 3, true, 1));
                            }
                            else
                            {
                                GameManager.Instance.currentDoobie.AddBuff(new Buff(BuffType.Bleed, 3, true, 1));
                            }
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
                    CurrentHealth = Mathf.Min(CurrentHealth + 1, MaxHealth);
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
}

