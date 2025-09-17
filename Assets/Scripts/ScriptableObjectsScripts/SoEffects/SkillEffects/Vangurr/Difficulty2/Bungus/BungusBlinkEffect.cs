using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty2/Bungus/BungusBlinkEffect")]
public class BungusBlinkEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveWeaponDamage();

        var (targetResult, actualTargetDmg) = target.TakeDamage(baseDmg, isSkill: true);

        // 50% chance
        if (UnityEngine.Random.value <= 0.5f)
        {
            target.AddEffect(new Effect(EffectType.Stun, 1, true, 1));
            target.AddEffect(new Effect(EffectType.Blind, 2, true, 1));


            return $"{user.CharacterName} blinks behind {target.CharacterName}, dealing {actualTargetDmg} damage, " +
                   $"stunning them for 1 turn and blinding them for 2 turns!";
        }

        return $"{user.CharacterName} blinks behind {target.CharacterName} and bites them, dealing {actualTargetDmg} damage!";
    }
}
