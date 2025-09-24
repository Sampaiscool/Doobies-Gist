using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Zelstine/Elenara/BlessedShieldEffect")]

public class BlessedShieldEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.BlessedShield, 5, false, (user.CurrentHealPower * 2)));

        return $"{user.CharacterName} Blesses herself gaining a blessed shield!";
    }
}
