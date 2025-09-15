using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/FatGremlin/NinjaLessionsEffect")]
public class NinjaLessionsEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddBuff(new Buff(BuffType.Evasion, 3, false, 1));

        // 25% chance to stun for 1 turn
        if (UnityEngine.Random.value <= 0.25f)
        {
            target.AddBuff(new Buff(BuffType.Stun, 1, true, 1));
            return $"{user.CharacterName} recalls their ninja lessons, gaining Evasion and stunning {target.CharacterName} for 1 turn!";
        }

        return $"{user.CharacterName} recalls their ninja lessons, gaining Evasion!";
    }
}
