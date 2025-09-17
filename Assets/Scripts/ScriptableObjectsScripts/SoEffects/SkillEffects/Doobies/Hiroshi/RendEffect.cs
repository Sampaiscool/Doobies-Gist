using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Hiroshi/Rend")]
public class RendEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddEffect(new Effect(EffectType.DefenceDown, 5, true ,1));
        return $"{user.CharacterName} rends {target.CharacterName}'s armor, reducing their defence for 5 turns!";
    }
}
