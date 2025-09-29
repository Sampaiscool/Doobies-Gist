using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Thengshou/WorldForm/FlashingKickEffect")]
public class FlashingKickEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        int halvedDmg = baseDmg / 2;

        // 50% chance to stun for 1 turn
        if (UnityEngine.Random.value <= 0.5f)
        {
            target.AddEffect(new Effect(EffectType.Stun, 3, true, 1));

            BattleUIManager.Instance.AddLog($"{user.CharacterName} stuns {target.CharacterName} with the kick!");
        }

        if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
        {
            soulflow.SpiritEnergy.Gain(2);
        }

        var (rewsult, damageDone) = target.TakeDamage(halvedDmg);

        return $"{user.CharacterName} kicks {target.CharacterName} dealing {damageDone} damage!";
    }
}
