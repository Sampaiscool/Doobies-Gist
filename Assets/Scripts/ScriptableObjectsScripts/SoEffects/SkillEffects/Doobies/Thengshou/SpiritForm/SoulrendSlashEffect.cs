using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Thengshou/SpiritForm/SoulrendSlashEffect")]
public class SoulrendSlashEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDmg = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg));

        int bonusDmg =+ (baseDmg / 2);

        target.AddEffect(new Effect(EffectType.Bleed, 4, true, 5));

        var (result, damageDone) = target.TakeDamage(bonusDmg, true, false, false, skill);

        if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
        {
            soulflow.WorldEnergy.Gain(2);
        }

        return $"{user.CharacterName} slashes {target.CharacterName} dealing {damageDone} damage and leaving them bleeding!";
    }
}
