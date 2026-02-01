using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/MushroomMan/MyceliumShieldEffect")]
public class MyceliumShieldEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int shieldAmount = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg) + user.GetEffectiveHealPower(user.CurrentHealPower));

        user.AddEffect(new Effect(EffectType.Shield, 5, false, shieldAmount));

        target.AddEffect(new Effect(EffectType.Spores, 5, false, (shieldAmount / 2)));

        return $"{user.CharacterName} grows a mycelium shield and attaches {shieldAmount / 2} spores to {target.CharacterName}!";
    }
}
