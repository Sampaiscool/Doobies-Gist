using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Bungus/BungusCurseEffect")]
public class BungusCurseEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddBuff(new Buff(BuffType.VampireCurse, 3, true, 1));

        return $"{user.CharacterName} curses {target.CharacterName} with vampirism";
    }
}
