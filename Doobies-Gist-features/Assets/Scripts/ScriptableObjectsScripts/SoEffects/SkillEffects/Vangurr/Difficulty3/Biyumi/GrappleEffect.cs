using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/Buyimu/GrappleEffect")]
public class GrappleEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        target.AddEffect(new Effect(EffectType.Stun, 2, true, 3));
        target.AddEffect(new Effect(EffectType.DefenceDown, 1, true, 3));

         string result = user.PerformBasicAttack(target);

        user.SetTransformation(Transformations.BiyumiNormal);

        BattleUIManager.Instance.AddLog($"{user.CharacterName} grabs {target.CharacterName}, stunning them and reducing their defence. They use a basic attack after this.");
        return result;
    }
}
