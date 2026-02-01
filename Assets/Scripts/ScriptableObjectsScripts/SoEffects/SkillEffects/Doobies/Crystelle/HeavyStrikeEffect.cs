using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Crystelle/HeavyStrike")]
public class HeavyStrikeEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDmg = Mathf.RoundToInt(user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage()));

        int doubleDmg = baseDmg * 2;

        var (result, damageDone) = target.TakeDamage(doubleDmg, true, false, false, skill);

        user.AddEffect(new Effect(EffectType.DefenceDown, 2, true));

        if (UnityEngine.Random.value <= 0.5f)
        {
            target.AddEffect(new Effect(EffectType.Stun, 2, true, 1));
            string basicResult = user.PerformBasicAttack(target);

            BattleUIManager.Instance.AddLog($"{user.CharacterName} performs a heavy strike on {target.CharacterName}, dealing {damageDone} damage, reducing their defence, and stunning them! They follow up with a basic attack.");
            return basicResult;
        }

        return $"{user.CharacterName} performs a heavy strike on {target.CharacterName}, dealing {damageDone} damage and reducing their defence.";
    }
}
