using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/MushroomMan/SporeCloudEffect")]
public class SporeCloudEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDamage = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg));

        int reduceAmount = baseDamage / 4;

        baseDamage = baseDamage - reduceAmount;

        int sporeAmount = 1;

        target.AddEffect(new Effect(EffectType.Spores, 5, true, 1));

        var (result, damageDone) = target.TakeDamage(baseDamage, true, false, false, skill);

        if (damageDone <= 5)
        {
            target.AddEffect(new Effect(EffectType.Spores, 5, true, damageDone));

            sporeAmount += damageDone;

            BattleUIManager.Instance.AddLog($"The damage was small so {user.CharacterName} attached some more spores!");
        }

        if (sporeAmount == 1)
        {
            return $"{user.CharacterName} activates a spore cloud dealing {damageDone} damage and attaching {sporeAmount} Spore!";
        }
        else
        {
            return $"{user.CharacterName} activates a spore cloud dealing {damageDone} damage and attaching {sporeAmount} Spores!";
        } 
    }
}
