using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Phrox/SapSiphonEffect")]
public class SapSiphonEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDamage = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg));

        int halvedDmg = Mathf.Max(1, baseDamage / 2);

        target.TakeDamage(halvedDmg, true, skill: skill);

        user.AddEffect(new Effect(EffectType.Regeneration, 2, false, halvedDmg));

        int healAmount = Mathf.RoundToInt((halvedDmg + user.CurrentHealPower) / 2f);

        user.HealCombatant(healAmount);

        return $"{user.CharacterName} Deals {halvedDmg} damage to {target.CharacterName} and steals some health!";
    }
}
