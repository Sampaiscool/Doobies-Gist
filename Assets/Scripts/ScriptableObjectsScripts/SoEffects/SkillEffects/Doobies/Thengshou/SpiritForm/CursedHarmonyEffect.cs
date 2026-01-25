using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Thengshou/SpiritForm/CursedHarmonyEffect")]
public class CursedHarmonyEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        user.AddEffect(new Effect(EffectType.SpellStrenghten, 5, false, 3));

        target.AddEffect(new Effect(EffectType.DefenceDown, 5, false, 3));

        if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
        {
            soulflow.WorldEnergy.Gain(2);
        }

        return $"{user.CharacterName} sings a cursed harmony; gaining Spell strenghten and inflicting {target.CharacterName} with defence down!";
    }
}
