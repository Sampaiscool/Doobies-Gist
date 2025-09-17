using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Phrox/CrimsonRootsEffect")]
public class CrimsonRootsEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddEffect(new Effect(EffectType.VampireCurse, 5, true, 1));

        user.AddEffect(new Effect(EffectType.WeaponWeaken, 10, true, 5));

        return $"{user.CharacterName} Fires its CrimsonRoot at the cost for its Weapon Damage!";
    }
}
