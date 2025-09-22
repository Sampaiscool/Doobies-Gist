using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/Lihm/StoneStareEffect")]
public class StoneStareEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        if (user.CurrentHealth <= (user.MaxHealth / 2))
        {
            user.AddEffect(new Effect(EffectType.Shield, 5, false, 15));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} are scared so they made a stone shield!");
        }
        else
        {
            user.AddEffect(new Effect(EffectType.HardHitter, 5, false, user.CurrentHealth));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} are ready to fight so they added some extra rocks to their weapon!");
        }

        target.AddEffect(new Effect(EffectType.Stun, 2, false, user.CurrentHealth));
        user.AddEffect(new Effect(EffectType.Harden, 4, false, (user.CurrentHealth / 2)));


        return $"{target.CharacterName} is in schock of the cold stare {user.CharacterName} trew!";
    }
}
