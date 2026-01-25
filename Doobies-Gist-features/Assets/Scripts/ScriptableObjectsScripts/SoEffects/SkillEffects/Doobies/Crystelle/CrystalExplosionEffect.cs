using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Crystelle/CrystalExplosion")]
public class CrystalExplosionEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        Effect crystalize = user.ActiveEffects.Find(c => c.type == EffectType.Crystalize);
        if (crystalize == null)
        {
            user.AddEffect(new Effect(EffectType.Crystalize, 10, false, 1));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} uses their crystalline power to harden");
        }
        else
        {
            user.AddEffect(new Effect(EffectType.Crystalize, 10, false, crystalize.intensity));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} uses their crystalline power to harden even more!");
        }

        int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        int startingDamage = baseDmg * 4;

        int roundedDefence = (int)user.CurrentDefence;

        int finalDmg = startingDamage * roundedDefence;

        var (result, damageDone) = target.TakeDamage(finalDmg, true, false, false, skill);

        target.AddEffect(new Effect(EffectType.Confused, 3, true, 1));

        return $"{user.CharacterName} channels their crystalline power to deal {damageDone} damage to {target.CharacterName}, leaving them confused!";
    }
}
