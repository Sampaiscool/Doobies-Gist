using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Phrox/SapSiphonEffect")]
public class SapSiphonEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDamage = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);

        int halvedDmg = Mathf.Max(1, baseDamage / 2);

        target.TakeDamage(halvedDmg, true);

        user.AddEffect(new Effect(EffectType.Regeneration, 2, false, halvedDmg));

        int healAmount = (halvedDmg + user.CurrentHealPower) /2 ;

        user.HealCombatant(healAmount);

        return $"{user.CharacterName} Deals {halvedDmg} damage to {target.CharacterName} and steals some health!";
    }
}
