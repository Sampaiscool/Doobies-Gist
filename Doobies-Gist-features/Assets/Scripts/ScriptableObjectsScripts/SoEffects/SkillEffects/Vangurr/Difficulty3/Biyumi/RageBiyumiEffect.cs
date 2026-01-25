using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/Buyimu/RageBiyumiEffect")]
public class RageBiyumiEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        if (user.CurrentTransformation != Transformations.BiyumiForm)
        {
            user.SetTransformation(Transformations.BiyumiForm);
            user.AddEffect(new Effect(EffectType.Rage, 5, false, 1));

            BattleUIManager.Instance.AddLog($"{user.CharacterName} raged! Transforming and gaining rage!");

            if (user is VangurrInstance vangurr)
            {
                vangurr.PerformTurn(target);
            }

        }
        else
        {
            user.AddEffect(new Effect(EffectType.Rage, 5, false, 2));
            return $"{user.CharacterName} raged but was already transfomred. They gained some extra rage!";
        }
        return $"{user.CharacterName} used rage!";
    }
}
