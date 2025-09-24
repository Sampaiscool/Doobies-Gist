using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Velithra/BlessedLightEffect")]
public class BlessedLightEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddEffect(new Effect(EffectType.Blind, 3, true, 3));
        target.AddEffect(new Effect(EffectType.WeaponWeaken, 3, true, 3));

        return $"{user.CharacterName} blinds {target.CharacterName} with a blessed light, blinding and weakening them for 3 turns!";
    }
}
