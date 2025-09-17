using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Phrox/WilttoWoundEffect")]
public class WilttoWoundEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.HealingStrenghten, 10, false, 1));

        return $"{user.CharacterName} Gains increased Healing power!";
    }
}
