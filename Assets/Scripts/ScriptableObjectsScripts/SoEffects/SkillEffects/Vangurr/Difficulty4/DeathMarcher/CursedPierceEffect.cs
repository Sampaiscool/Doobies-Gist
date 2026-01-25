using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty4/DeathMarcher/CursedPierceEffect")]
public class CursedPierceEffect : SkillEffectSO
{
	public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
	{
		int baseDamage = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

		user.AddEffect(new Effect(EffectType.Regeneration, 2, false, baseDamage / 3));

		var (targetResult, actualTargetDmg) = target.TakeDamage(baseDamage, true, false, true, skill);
		string result = user.PerformBasicAttack(target);

		target.AddEffect(new Effect(EffectType.VanishedDefense, 2, true, 1));

		BattleUIManager.Instance.AddLog($"{user.CharacterName} pierces the soul of {target.CharacterName}, dealing {actualTargetDmg} damage!");
		return result;
	}
}
