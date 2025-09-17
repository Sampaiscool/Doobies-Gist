using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/FatGremlin/MeatShieldEffect")]
public class MeatShieldEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Harden, 5, true, 2));

        return $"{user.CharacterName} surrounds themselves with a shield of meat, gaining 2 Harden for 5 turns!";
    }
}
