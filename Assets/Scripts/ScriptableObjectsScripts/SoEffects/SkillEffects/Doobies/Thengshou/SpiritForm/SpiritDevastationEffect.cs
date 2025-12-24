using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Thengshou/SpiritForm/SpiritDevastationEffect")]
public class SpiritDevastationEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);

        int bonusDmg =+ (baseDmg / 2);

        if (target.CurrentHealth <= (target.MaxHealth / 4))
        {
            bonusDmg *= 2;
            BattleUIManager.Instance.AddLog($"{target.CharacterName} is low on health so spirit devastation deals more damage!");
        }

        var (result, damageDone) = target.TakeDamage(bonusDmg, true);

        if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
        {
            soulflow.WorldEnergy.Gain(2);
        }

        return $"{user.CharacterName} devastates {target.CharacterName} spirit dealing {damageDone} damage!";
    }
}
