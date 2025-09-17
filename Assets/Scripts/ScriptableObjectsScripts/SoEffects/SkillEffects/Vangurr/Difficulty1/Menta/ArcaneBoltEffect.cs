using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/Menta/ArcaneBoltEffect")]
public class ArcaneBoltEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.CurrentSkillDmg + 2;

        int effectiveDmg = user.GetEffectiveSkillDamage(baseDmg);

        var (targetResult, actualTargetDmg) = target.TakeDamage(effectiveDmg, isSkill: true);

        return $"{user.CharacterName} fires an arcane bolt at {target.CharacterName}, dealing {actualTargetDmg} damage!";
    }
}
