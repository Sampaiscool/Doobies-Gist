using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty2/FatGremlin/NinjaLessionsEffect")]
public class NinjaLessionsEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Evasion, 3, false, 1));

        // 25% chance to stun for 1 turn
        if (UnityEngine.Random.value <= 0.25f)
        {
            target.AddEffect(new Effect(EffectType.Stun, 1, true, 1));
            return $"{user.CharacterName} recalls their ninja lessons, gaining Evasion and stunning {target.CharacterName} for 1 turn!";
        }

        return $"{user.CharacterName} recalls their ninja lessons, gaining Evasion!";
    }
}
