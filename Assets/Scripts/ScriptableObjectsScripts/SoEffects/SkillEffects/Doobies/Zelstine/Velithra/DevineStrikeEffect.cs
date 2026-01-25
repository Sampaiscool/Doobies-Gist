using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Velithra/DevineStrikeEffect")]
public class DevineStrikeEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDamage = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);

        var (result, damageDone) = target.TakeDamage(baseDamage, true, false, false, skill);

        target.AddEffect(new Effect(EffectType.Holy, 3, true, damageDone));

        return $"{user.CharacterName} strikes {target.CharacterName} dealing {damageDone} damage and inflicting {damageDone} Holy!";
    }
}
