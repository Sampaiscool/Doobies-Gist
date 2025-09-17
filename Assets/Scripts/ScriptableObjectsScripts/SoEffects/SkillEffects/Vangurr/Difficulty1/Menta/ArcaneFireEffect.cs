using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/Menta/ArcaneFireEffect")]
public class ArcaneFireEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDamage = user.CurrentSkillDmg;
        
        int effectiveDamage = user.GetEffectiveSkillDamage(baseDamage);

        target.TakeDamage(effectiveDamage);

        target.AddEffect(new Effect(EffectType.Burn, 5, true, 2));

        return $"{user.CharacterName} hurls a ball of arcane fire at {target.CharacterName}, dealing {effectiveDamage} damage and inflicting Burn!";
    }
}
