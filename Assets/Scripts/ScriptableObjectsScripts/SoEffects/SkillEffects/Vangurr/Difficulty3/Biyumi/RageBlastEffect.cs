using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/Buyimu/RageBlastEffect")]
public class RageBlastEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        target.AddEffect(new Effect(EffectType.VampireCurse, 5, true, 1));

        int baseDmg = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);

        var (r, damageDone) = target.TakeDamage(baseDmg);

        string result = user.PerformBasicAttack(target);

        user.SetTransformation(Transformations.BiyumiNormal);

        BattleUIManager.Instance.AddLog($"{user.CharacterName} blasts {target.CharacterName}, giving them vampire curse and dealing {damageDone} damage! They use a basic attack after this.");
        return result;
    }
}
