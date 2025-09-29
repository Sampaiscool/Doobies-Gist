using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/Buyimu/BlitzFlashEffect")]
public class BlitzFlashEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        int modifiedDmg = baseDmg / baseDmg;

        BattleUIManager.Instance.AddLog($"{user.CharacterName} attacks {target.CharacterName} quickly!");

        for (int i = 0; i < 3; i++)
        {
            var (result, damageDone) = target.TakeDamage(modifiedDmg);
            BattleUIManager.Instance.AddLog($"{target.CharacterName} takes {damageDone} damage!");
        }

        user.AddEffect(new Effect(EffectType.Harden, 5, false, 3));

        user.SetTransformation(Transformations.None);

        return $"{user.CharacterName} also hardens it's skin";
    }
}
