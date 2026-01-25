using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Kaelyth/SinEffect")]
public class SinEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        user.AddEffect(new Effect(EffectType.Bleed, 2, true, 5));
        user.AddEffect(new Effect(EffectType.DefenceDown, 2, true, 2));

        int baseDamage = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        baseDamage *= 2;

        var (result, damageDone) = target.TakeDamage(baseDamage, true, false, false, skill);

        return $"{user.CharacterName} repents for their sins, gaining bleed and defence down. they deal {damageDone} damage to {target.CharacterName}";
    }
}
