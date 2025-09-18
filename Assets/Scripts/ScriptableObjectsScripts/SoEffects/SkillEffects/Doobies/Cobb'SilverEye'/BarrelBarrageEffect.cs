using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/CobbSilverEye/BarrelBarrageEffect")]
public class BarrelBarrageEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Barrel, 100, false, 5));

        Effect userBarrelBuff = user.ActiveEffects.Find(b => b.type == EffectType.Barrel);
        Effect targetBarrelBuff = user.ActiveEffects.Find(b => b.type == EffectType.Barrel);
        if (userBarrelBuff != null)
        {
            int allBarrels = userBarrelBuff.intensity + targetBarrelBuff.intensity;

            for (int i = 0; i < allBarrels; i++)
            {
                target.AddEffect(new Effect(EffectType.Burn, 1, true, 1));
            }
        }

        user.ExplodeBarrels(user, target, false, false);

        return $"{user.CharacterName} Placed some barrels and exploded all barrels";
    }
}
