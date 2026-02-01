using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Kaelyth/SkillfullBashEffect")]
public class SkillfullBashEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDamage = Mathf.RoundToInt(user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage()));

        // 50% chance to stun for 1 turn
        if (UnityEngine.Random.value <= 0.5f)
        {
            target.AddEffect(new Effect(EffectType.Stun, 2, true, 1));
            user.AddEffect(new Effect(EffectType.HardHitter, 4, true, 1));

            BattleUIManager.Instance.AddLog($"{user.CharacterName} stuns {target.CharacterName} with the bash and gains Hard Hitter!");
        }

        var (result, damageDone) = target.TakeDamage(baseDamage, true, false, false, skill);

        user.AddEffect(new Effect(EffectType.Deflecion, 999, true, damageDone));

        return $"{user.CharacterName} bashes {target.CharacterName} and deals {damageDone} and gaining that much Deflection";
    }
}
