using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/Menta/ArcaneShield")]
public class ArcaneShield : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Shield, 999, false, 3));

        return $"{user.CharacterName} conjures an arcane shield, absorbing incoming damage!";
    }
}
