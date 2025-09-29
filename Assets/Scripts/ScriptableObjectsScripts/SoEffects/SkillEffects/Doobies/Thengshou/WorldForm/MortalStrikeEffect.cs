using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Thengshou/WorldForm/MortalStrikeEffect")]
public class MortalStrikeEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        var (result, damageDone) = target.TakeDamage(baseDmg);

        if (damageDone <= (target.MaxHealth / 2))
        {
            target.AddEffect(new Effect(EffectType.TargetLocked, 3, true, damageDone));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} attacks {target.CharacterName} with a mortal strike; dealing {damageDone} damage!");
            return $"The damage was not enough for {user.CharacterName} so they gave {target.CharacterName} {damageDone} target locked!";
        }

        if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
        {
            soulflow.SpiritEnergy.Gain(2);
        }

        return $"{user.CharacterName} attacks {target.CharacterName} with a mortal strike; dealing {damageDone} damage";
    }
}
