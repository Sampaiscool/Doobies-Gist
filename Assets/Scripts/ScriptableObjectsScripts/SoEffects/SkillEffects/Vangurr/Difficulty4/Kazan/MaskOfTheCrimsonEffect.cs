using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty4/Kazan/MaskOfTheCrimsonEffect")]
public class MaskOfTheCrimsonEffect : SkillEffectSO
{
	public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
	{
		user.AddEffect(new Effect(EffectType.HealingStrenghten, 5, false, 3));
		user.AddEffect(new Effect(EffectType.Regeneration, 3, false, 1));

		target.AddEffect(new Effect(EffectType.WeaponWeaken, 3, true, 2));
		target.AddEffect(new Effect(EffectType.SpellWeaken, 3, true, 2));

		//Val basic aan als je eigen HP minder dan de helft wordt
		if(user.CurrentHealth <= (user.MaxHealth / 2))
		{
			target.AddEffect(new Effect(EffectType.CrimsonCurse, 3, true, 1));
			user.PerformBasicAttack(target);

			BattleUIManager.Instance.AddLog($"{user.CharacterName} floods the floor with crimson, performing a basic attack and inflicting 1 Crimson Curse");
		}

		return ($"{user.CharacterName} shows his mask to {target.CharacterName}!");
	}
}
