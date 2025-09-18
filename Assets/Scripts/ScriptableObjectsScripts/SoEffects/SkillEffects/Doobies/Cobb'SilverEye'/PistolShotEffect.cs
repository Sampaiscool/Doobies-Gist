using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/CobbSilverEye/PistolShotEffect")]
public class PistolShotEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.ExplodeBarrels(user, target, false, true);

        return $"{user.CharacterName} Uses their pistol shot!";
    }
}
