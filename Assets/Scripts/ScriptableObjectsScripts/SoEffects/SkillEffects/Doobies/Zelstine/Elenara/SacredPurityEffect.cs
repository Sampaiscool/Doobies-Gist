using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Elenara/SacredPurityEffect")]
public class SacredPurityEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int debuffsRemoved = 0;
        
        foreach (var effect in user.ActiveEffects)
        {
            if (effect.isDebuff == true)
            {
                user.ActiveEffects.Remove(effect);
                debuffsRemoved++;
            }
        }

        if (debuffsRemoved >= 3)
        {
            int healed = user.HealCombatant(debuffsRemoved * 2);
            BattleUIManager.Instance.AddLog($"{user.CharacterName} removed enough debuffs to purify themself, healing {healed} health!");
        }
        return $"{user.CharacterName} has blessed the sacred air around them; removing every debuff they had!";
    }
}
