using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/MisterEraser/Ereasure")]
public class EreasureEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddBuff(new Buff(BuffType.Weaken, 2, true, 1));
        return $"{user.CharacterName} erases some of {target.CharacterName}'s power!";
    }
}
