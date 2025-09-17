using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/MisterEraser/Ereasure")]
public class EreasureEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddEffect(new Effect(EffectType.WeaponWeaken, 2, true, 1));
        target.AddEffect(new Effect(EffectType.SpellWeaken, 2, true, 1));
        return $"{user.CharacterName} erases some of {target.CharacterName}'s power!";
    }
}
