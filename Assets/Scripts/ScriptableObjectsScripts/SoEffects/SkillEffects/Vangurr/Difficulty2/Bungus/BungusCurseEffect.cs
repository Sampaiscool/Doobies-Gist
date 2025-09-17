using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty2/Bungus/BungusCurseEffect")]
public class BungusCurseEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddEffect(new Effect(EffectType.VampireCurse, 3, true, 1));

        return $"{user.CharacterName} curses {target.CharacterName} with vampirism";
    }
}
