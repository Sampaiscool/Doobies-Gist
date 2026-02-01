using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Thengshou/SpiritForm/PhantomRushEffect")]
public class PhantomRushEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDmg = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg));

        int bonusDmg =+ (baseDmg / 2);

        var (result, damageDone) = target.TakeDamage(bonusDmg, true, false, false, skill);

        user.AddEffect(new Effect(EffectType.Hidden, 1, false, bonusDmg));
        user.AddEffect(new Effect(EffectType.Evasion, 2, false, bonusDmg));

        if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
        {
            soulflow.WorldEnergy.Gain(2);
        }

        return $"{user.CharacterName} rushes at {target.CharacterName} dealing {damageDone} damage! After this {user.CharacterName} becomes hidden and gaining evasion!";
    }
}
