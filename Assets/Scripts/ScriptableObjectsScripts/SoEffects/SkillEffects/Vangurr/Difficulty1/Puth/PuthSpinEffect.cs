using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/Puth/PuthSpinEffect")]
public class PuthSpinEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Stun, 2, true, 1));

        target.AddEffect(new Effect(EffectType.DefenceDown, 3, true, 5));
        target.AddEffect(new Effect(EffectType.TargetLocked, 3, true, 3));

        return $"{user.CharacterName} Spins around activating a magical effect.";
    }
}
