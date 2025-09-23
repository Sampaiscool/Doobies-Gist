using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Elenara/BlessingOfRenewalEffect")]
public class BlessingOfRenewalEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        if (user is DoobieInstance doobie)
        {
            int faithAmount = doobie.MainResource.Max;

            faithAmount /= 4;

            int healedAmount = user.HealCombatant(faithAmount);

            user.AddEffect(new Effect(EffectType.Shield, 10, false, user.CurrentSkillDmg));

            return $"{user.CharacterName} blesses herself grating a shield and healing them for {healedAmount}";
        }

        return "wat";
    }
}
