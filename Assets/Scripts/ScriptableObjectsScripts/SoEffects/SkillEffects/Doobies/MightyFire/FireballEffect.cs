using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/MightyFire/FireballEffect")]
public class FireballEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        float damageF = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);
        int damage = Mathf.RoundToInt(damageF);

        var (result, damageDone) = target.TakeDamage(damage, true, false, false, skill);

        target.AddEffect(new Effect(EffectType.Burn, 3, true, (damageDone / 2)), source: user);

        return $"{user.CharacterName} unleashes a fireball dealing {damageDone} damage to {target.CharacterName} and giving them {damageDone / 2} burn!";
    }
}
