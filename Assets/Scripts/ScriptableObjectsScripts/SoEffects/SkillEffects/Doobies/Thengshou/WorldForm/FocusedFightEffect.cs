using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Thengshou/WorldForm/FocusedFightEffect")]
public class FocusedFightEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int healPower = Mathf.RoundToInt(user.GetEffectiveHealPower(user.CurrentHealPower));

        user.AddEffect(new Effect(EffectType.Shield, 5, false, healPower));

        if (user.CurrentHealth <= (user.MaxHealth) / 2)
        {
            user.AddEffect(new Effect(EffectType.Shield, 5, false, healPower));
            user.AddEffect(new Effect(EffectType.Harden, 5, false, healPower));

            if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
            {
                soulflow.SpiritEnergy.Gain(2);
            }

            BattleUIManager.Instance.AddLog($"{user.CharacterName} shields themself again for more incoming damage!");
            return $"{user.CharacterName} also improves her defence!!";
        }
        else
        {
            if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
            {
                soulflow.SpiritEnergy.Gain(2);
            }

            return $"{user.CharacterName} shields themself for incoming damage!";
        }
    }
}
