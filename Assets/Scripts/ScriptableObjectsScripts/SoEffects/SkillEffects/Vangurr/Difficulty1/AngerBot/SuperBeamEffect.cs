using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/AngerBot/SuperBeamEffect")]
public class SuperBeamEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        // Base skill damage before buffs/debuffs
        int baseDmg = user.CurrentSkillDmg;

        // Apply modifiers (Weaken, etc.)
        int effectiveDmg = user.GetEffectiveSkillDamage(baseDmg);

        // Flat bonus for this skill
        int finalDmg = effectiveDmg + 3;

        // Deal damage to the target
        var (targetResult, actualTargetDmg) = target.TakeDamage(finalDmg, isSkill: true);

        // Self-recoil is half of final damage dealt
        int recoil = Mathf.Max(1, finalDmg / 2);
        var (selfResult, actualSelfDmg) = user.TakeDamage(recoil, isSkill: true);

        return $"{user.CharacterName} fires a super beam at {target.CharacterName}, " +
               $"dealing {actualTargetDmg} damage!\n" +
               $"{user.CharacterName} also takes {actualSelfDmg} recoil damage!";
    }
}

