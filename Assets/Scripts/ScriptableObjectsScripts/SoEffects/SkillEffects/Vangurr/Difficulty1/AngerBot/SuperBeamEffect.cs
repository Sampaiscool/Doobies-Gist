using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/AngerBot/SuperBeamEffect")]
public class SuperBeamEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        // Base skill damage before buffs/debuffs
        int baseDmg = Mathf.RoundToInt(user.CurrentSkillDmg);

        // Apply modifiers (Weaken, etc.)
        int effectiveDmg = Mathf.RoundToInt(user.GetEffectiveSkillDamage(baseDmg));

        // Flat bonus for this skill
        int finalDmg = effectiveDmg + 3;

        // Deal damage to the target
        var (targetResult, actualTargetDmg) = target.TakeDamage(finalDmg, isSkill: true, skill: skill);

        // Self-recoil is half of final damage dealt
        int recoil = Mathf.Max(1, finalDmg / 2);
        var (selfResult, actualSelfDmg) = user.TakeDamage(recoil, isSkill: true, skill: skill);

        return $"{user.CharacterName} fires a super beam at {target.CharacterName}, " +
               $"dealing {actualTargetDmg} damage!\n" +
               $"{user.CharacterName} also takes {actualSelfDmg} recoil damage!";
    }
}

