using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty4/Kazan/CrimsonsHeartEffect")]
public class CrimsonsHeartEffect : SkillEffectSO
{
	public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
	{
		int baseDmg = 0;
		int currentSkillPower = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);

		int totalBurn = target.GetTotalEffectIntensity(EffectGroup.BurnLike);
		if (totalBurn > 0)
		{
			baseDmg = totalBurn + currentSkillPower;
			var (result, damageDone) = target.TakeDamage(baseDmg, true, false, false, skill);
			return $"{user.CharacterName} heart burns brightly, dealing {damageDone} to {target.CharacterName}!";
		}
		else
		{
			target.AddEffect(new Effect(EffectType.Burn, 10, true, (currentSkillPower * 2)));
			return $"{user.CharacterName} heart burns brighter than the sun, giving {target.CharacterName} {currentSkillPower * 2} burn!";
		}
	}
}
