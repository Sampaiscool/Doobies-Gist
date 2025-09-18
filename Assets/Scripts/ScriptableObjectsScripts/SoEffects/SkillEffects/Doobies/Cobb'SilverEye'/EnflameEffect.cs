using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/CobbSilverEye/EnflameEffect")]
public class EnflameEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Enflame, 5, false, 1));

        return $"{user.CharacterName} Enflames his weapon!";
    }
}
