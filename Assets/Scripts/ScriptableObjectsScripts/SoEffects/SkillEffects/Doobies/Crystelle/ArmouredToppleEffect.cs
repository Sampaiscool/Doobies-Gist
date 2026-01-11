using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Crystelle/ArmouredTopple")]
public class ArmouredToppleEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        user.AddEffect(new Effect(EffectType.WeaponWeaken, 1, true, 4));
        user.AddEffect(new Effect(EffectType.Harden, 2, true, 1));

        string basicResult = user.PerformBasicAttack(target);

        if (UnityEngine.Random.value <= 0.75f)
        {
            target.AddEffect(new Effect(EffectType.Confused, 4, true, 1));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} topples their armour, weakening their weapon and hardening themselves before attacking {target.CharacterName}. {target.CharacterName} is confused by the sudden attack! \n{basicResult}");
            return basicResult;
        }
        BattleUIManager.Instance.AddLog($"{user.CharacterName} topples their armour, weakening their weapon and hardening themselves before attacking {target.CharacterName}");
        return basicResult;
    }
}
