using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty2/BulletJunior/RemingtonSnipeEffect")]
public class RemingtonSnipeEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        int critChanceUser = user.GetEffectiveCritChanceAfterEffects(user.GetEffectiveCritChance());

        int critNumber = Random.Range(0, 100);

        if (critNumber <= critChanceUser)
        {
            baseDmg *= 2;
            BattleUIManager.Instance.AddLog($"{user.CharacterName} Hits a CRITICAl attack");
        }

        var (result, damageDone) = target.TakeDamage(baseDmg, true, false);

        target.AddEffect(new Effect(EffectType.Bleed, 2, true, damageDone));

        return $"{user.CharacterName} Snipes {target.CharacterName} dealing {damageDone} damage and inflicting {damageDone} bleed!";
    }
}
