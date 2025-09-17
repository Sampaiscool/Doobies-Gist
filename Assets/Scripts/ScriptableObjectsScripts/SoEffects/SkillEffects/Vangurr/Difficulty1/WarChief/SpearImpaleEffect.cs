using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty1/WarChief/SpearImpaleEffect")]
public class SpearImpaleEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.WeaponStrenghten, 2, false, 1));

        int baseDmg = user.GetEffectiveWeaponDamage();

        var (targetResult, actualTargetDmg) = target.TakeDamage(baseDmg, isSkill: true);

        return $"{user.CharacterName} impales {target.CharacterName} gaining extra weapon strenght and dealing {actualTargetDmg} damage!";
    }
}
