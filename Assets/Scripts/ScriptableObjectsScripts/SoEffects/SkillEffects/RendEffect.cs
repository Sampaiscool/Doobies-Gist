using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Rend")]
public class RendEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddBuff(new Buff(BuffType.DefenceDown, 5, true ,1));
        return $"{user.CharacterName} rends {target.CharacterName}'s armor, reducing their defence by 20% for 5 turns!";
    }
}
