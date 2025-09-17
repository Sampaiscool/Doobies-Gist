using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Phrox/GardenOfThornsEffect")]
public class GardenOfThornsEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.HealingWeaken, 5, true, 2));

        int baseDmg = user.GetEffectiveSkillDamage(user.CurrentSkillDmg) + user.GetEffectiveHealPower(user.CurrentHealPower);

        user.HealCombatant(baseDmg / 2);

        var (result, damageDone) = target.TakeDamage(baseDmg, true);

        BattleUIManager.Instance.AddLog($"{user.CharacterName} Enters the Garden of Thorns reducing their healing");
        return $"{user.CharacterName} deals {damageDone} damage!";
    }
}
