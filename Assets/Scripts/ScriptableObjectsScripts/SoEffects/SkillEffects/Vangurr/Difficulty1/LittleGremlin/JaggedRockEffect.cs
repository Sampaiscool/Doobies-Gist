using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/LittleGremlin/JaggedRockEffect")]
public class JaggedRockEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDmg = Mathf.RoundToInt(user.CurrentSkillDmg);

        int effectiveDmg = Mathf.RoundToInt(user.GetEffectiveSkillDamage(baseDmg));

        int halvedDmg = Mathf.Max(1, effectiveDmg / 2);

        var (targetResult, actualTargetDmg) = target.TakeDamage(halvedDmg, isSkill: true, skill: skill);

        // 50% chance to stun for 1 turn
        if (UnityEngine.Random.value <= 0.5f)
        {
            target.AddEffect(new Effect(EffectType.Stun, 2, true, 1));
            return $"{user.CharacterName} hurls a jagged rock at {target.CharacterName}, " +
                   $"dealing {actualTargetDmg} damage and stunning them for 1 turn!";
        }

        return $"{user.CharacterName} hurls a jagged rock at {target.CharacterName}, " +
               $"dealing {actualTargetDmg} damage!";
    }
}
