using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Velithra/FatedCurseEffect")]
public class FatedCurseEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddEffect(new Effect(EffectType.HealingWeaken, 3, true, 3));
        target.AddEffect(new Effect(EffectType.SpellWeaken, 3, true, 3));

        return $"{user.CharacterName} activates a holy curse on {target.CharacterName}, granting healing weaken and spell weaken for 3 turn!";
    }
}
