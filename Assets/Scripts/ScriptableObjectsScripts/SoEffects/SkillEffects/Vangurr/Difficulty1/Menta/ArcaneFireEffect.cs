using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/Menta/ArcaneFireEffect")]
public class ArcaneFireEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDamage = Mathf.RoundToInt(user.CurrentSkillDmg);
        
        int effectiveDamage = Mathf.RoundToInt(user.GetEffectiveSkillDamage(baseDamage));

        target.TakeDamage(effectiveDamage, true, skill: skill);

        target.AddEffect(new Effect(EffectType.Burn, 5, true, 2));

        return $"{user.CharacterName} hurls a ball of arcane fire at {target.CharacterName}, dealing {effectiveDamage} damage and inflicting Burn!";
    }
}
