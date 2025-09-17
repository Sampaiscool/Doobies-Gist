using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Phrox/VerdantFangsEffect")]
public class VerdantFangsEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);

        var (result, damageDealt)  = target.TakeDamage(baseDmg, true);

        // If damage was less then half of targets Max HP
        if (damageDealt <= (target.MaxHealth / 2))
        {
            int baseBonusDmg = user.GetEffectiveHealPower(user.CurrentHealPower);

            target.AddEffect(new Effect(EffectType.HealingWeaken, 1, true, baseBonusDmg));

            // Both heal
            user.HealCombatant(baseBonusDmg);
            target.HealCombatant(target.GetEffectiveHealPower(target.CurrentHealPower));

            BattleUIManager.Instance.AddLog($"{user.CharacterName} Charges at {target.CharacterName}, dealing {damageDealt} damage!");
            return $"{user.CharacterName} was not satisfied with the result and healed both {user.CharacterName} and {target.CharacterName}";
        }
        else
        {
            return ($"{user.CharacterName} Charges at {target.CharacterName}, dealing {damageDealt} damage!");
        }
    }
}
