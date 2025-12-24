using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Kaelyth/GoddessPunchEffect")]
public class GoddessPunchEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDamage = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);

        baseDamage += Mathf.FloorToInt(target.MaxHealth / 10);

        baseDamage *= 2;

        var (result, damageDone) = target.TakeDamage(baseDamage, true);

        if (damageDone >= (target.MaxHealth / 2))
        {
            BattleUIManager.Instance.AddLog($"Kaelyth Attacks with {user.CharacterName} dealing {damageDone} damage!");
            BattleUIManager.Instance.AddLog($"{user.CharacterName} deals enough damage to gain an extra basic attack!");
            string log = user.PerformBasicAttack(target);
            return log;
        }
        return $"Kaelyth Attacks with {user.CharacterName} dealing {damageDone} damage!";
    }
}
