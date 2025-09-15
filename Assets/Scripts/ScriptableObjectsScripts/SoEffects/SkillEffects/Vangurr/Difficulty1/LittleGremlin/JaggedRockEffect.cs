using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/LittleGremlin/JaggedRockEffect")]
public class JaggedRockEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.CurrentSkillDmg;

        int effectiveDmg = user.GetEffectiveSkillDamage(baseDmg);

        int halvedDmg = Mathf.Max(1, effectiveDmg / 2);

        var (targetResult, actualTargetDmg) = target.TakeDamage(halvedDmg, isSkill: true);

        // 50% chance to stun for 1 turn
        if (UnityEngine.Random.value <= 0.5f)
        {
            target.AddBuff(new Buff(BuffType.Stun, 1, true, 1));
            return $"{user.CharacterName} hurls a jagged rock at {target.CharacterName}, " +
                   $"dealing {actualTargetDmg} damage and stunning them for 1 turn!";
        }

        return $"{user.CharacterName} hurls a jagged rock at {target.CharacterName}, " +
               $"dealing {actualTargetDmg} damage!";
    }
}
