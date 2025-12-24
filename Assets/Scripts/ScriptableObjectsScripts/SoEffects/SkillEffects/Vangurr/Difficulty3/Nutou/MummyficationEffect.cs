using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/Nutou/MummyficationEffect")]
public class MummyficationEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        target.AddEffect(new Effect(EffectType.VampireCurse, 1, true, (baseDmg / 2)));

        var (result1, damageDone1) = target.TakeDamage(baseDmg, true);

        if (baseDmg <= (target.MaxHealth / 2))
        {
            target.AddEffect(new Effect(EffectType.NutouCurse, 1, true, (baseDmg / 2)));

            var (result2, damageDone2) = target.TakeDamage(baseDmg, true);

            BattleUIManager.Instance.AddLog($"{user.CharacterName} Became a mummy and attacked {target.CharacterName} for {damageDone1} damage!");
            return $"The damage was not enough for {user.CharacterName} so he cursed {target.CharacterName} and attacked for {damageDone2} damage!";
        }

        return $"{user.CharacterName} Became a mummy and attacked {target.CharacterName} for {damageDone1} damage!";
    }
}
