using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty2/GiggyGrass/HiddenGrassEffect")]
public class HiddenGrassEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Hidden, 3, true, 1));

        user.AddEffect(new Effect(EffectType.CriticalEye, 4, true, 2));

        target.AddEffect(new Effect(EffectType.TargetLocked, 3, false, user.GetEffectiveSkillDamage(user.CurrentSkillDmg)));


        BattleUIManager.Instance.AddLog($"{user.CharacterName} Hides in the grass, gaining Hidden and CriticalEye.");

        return $"{target.CharacterName} is targeted by the grass to take damage.";
    }
}
