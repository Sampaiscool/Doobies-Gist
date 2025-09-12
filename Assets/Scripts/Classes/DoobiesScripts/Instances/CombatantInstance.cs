using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CombatantInstance
{

    public Transform animationAnchor;

    public abstract ScriptableObject so { get; }
    public abstract string CharacterName { get; }
    public abstract int CurrentHealth { get; set; }
    public abstract float CurrentDefence { get; set; }

    public WeaponInstance EquippedWeaponInstance;

    public int GetEffectiveWeaponDamage() => EquippedWeaponInstance?.GetEffectiveDamage() ?? 0;
    public int GetEffectiveCritChance() => EquippedWeaponInstance?.GetEffectiveCritChance() ?? 0;

    public abstract List<SkillSO> GetAllSkills();
    public List<Buff> ActiveBuffs { get; private set; } = new List<Buff>();
    public List<GameObject> ActiveBuffIcons = new List<GameObject>();


    /// <summary>
    /// The instance takes damage, reduced by defence.
    /// </summary>
    /// <param name="amount">The amount of damage before defence</param>
    /// <param name="isSkill">wheter the dmg came from a skill</param>
    /// <returns></returns>
    public virtual DamageResult TakeDamage(int amount, bool isSkill = false)
    {
        // Eerst kijken of er deflection is
        if (!isSkill && HandleDeflection())
        {
            return DamageResult.Deflected;
        }

        float defence = GetEffectiveDefence();
        int reducedDamage = Mathf.CeilToInt(amount / defence);

        CurrentHealth = Mathf.Max(CurrentHealth - reducedDamage, 0);
        return DamageResult.Hit;
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
        }

        return true; // een deflect is afgehandeld
    }

    /// <summary>
    /// The instance preforms a basic attack and deals damage to the target if posible
    /// </summary>
    /// <param name="target">The instance that is egtting attacked</param>
    /// <returns>String that the combat log needs</returns>
    public virtual string PerformBasicAttack(CombatantInstance target)
    {
        if (EquippedWeaponInstance == null)
            return $"{CharacterName} tries to attack, but is unarmed!";

        var attack = EquippedWeaponInstance.BasicAttack;

        if (Random.value < EquippedWeaponInstance.MissChance)
        {
            return $"{CharacterName} swings at {target.CharacterName}, but misses!";
        }

        float multiplier = Random.Range(0.5f, 1.5f);
        int baseDamage = Mathf.RoundToInt(attack.damage * multiplier);

        bool isCrit = Random.Range(0, 100) < GetEffectiveCritChance();
        int finalDamage = isCrit ? baseDamage * 2 : baseDamage;

        // Activate Effects
        if (EquippedWeaponInstance.Animation != null)
        {
            target.PlayAttackAnimation(EquippedWeaponInstance.Animation);
        }

        DamageResult result = target.TakeDamage(finalDamage);

        switch (result)
        {
            case DamageResult.Deflected:
                return $"{CharacterName} strikes, but {target.CharacterName} deflects the blow with finesse!";
            case DamageResult.Hit:
                return isCrit
                    ? $"{CharacterName} lands a CRITICAL HIT on {target.CharacterName} for {finalDamage} damage!"
                    : $"{CharacterName} strikes {target.CharacterName} for {finalDamage} damage!";
            case DamageResult.Missed:
                return $"{CharacterName}'s attack phases through thin air!";
            case DamageResult.Immune:
                return $"{target.CharacterName} is immune to the attack!";
            case DamageResult.Blocked:
                return $"{target.CharacterName} blocks the hit and takes no damage!";
            default:
                return $"{CharacterName} attacks, but something strange happens...";
        }
    }
    public void AddBuff(Buff newBuff)
    {
        // Look for an existing buff of the same type
        Buff existing = ActiveBuffs.Find(b => b.type == newBuff.type);

        if (existing != null)
        {
            // Stack them: add duration and intensity
            existing.duration += newBuff.duration;
            existing.intensity += newBuff.intensity;
        }
        else
        {
            // Otherwise just add it fresh
            ActiveBuffs.Add(newBuff);
        }

        // Update the UI immediately so the change is visible
        BattleUIManager.Instance.UpdateUI();
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
        spawned.transform.localScale = new Vector3(75, 75, 75);

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

