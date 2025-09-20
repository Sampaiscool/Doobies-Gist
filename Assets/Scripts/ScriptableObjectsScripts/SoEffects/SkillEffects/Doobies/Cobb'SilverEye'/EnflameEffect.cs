using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/CobbSilverEye/EnflameEffect")]
public class EnflameEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Enflame, 5, false, 1));

        bool extraEffect = Random.Range(0, 100) < user.GetEffectiveCritChanceAfterEffects(user.GetEffectiveCritChance());

        if (extraEffect)
        {
            user.AddEffect(new Effect(EffectType.HardHitter, 5, false, 1));

            BattleUIManager.Instance.AddLog($"Enflame had a CRITICAl effect!");
        }

        return $"{user.CharacterName} Enflames his weapon!";
    }
}
