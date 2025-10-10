using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty4/Kazan/CrimsonTangleEffect")]
public class CrimsonTangleEffect : SkillEffectSO
{
	public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
	{
		target.AddEffect(new Effect(EffectType.Stun, 2, true, 1));

		int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

		var (result, damageDone) = target.TakeDamage(baseDmg, true);

		int healing = user.HealCombatant(baseDmg);

		return ($"{user.CharacterName} wraps {target.CharacterName} in his crimson robe, dealing {damageDone} damage, stunning them and healing {user.CharacterName} for {healing} HP!");
	}
}
