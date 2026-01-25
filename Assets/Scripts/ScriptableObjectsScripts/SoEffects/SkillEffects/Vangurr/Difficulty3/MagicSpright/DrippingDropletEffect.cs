using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/MagicSpright/DrippingDropletEffect")]
public class DrippingDropletEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDmg = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);
        int totalDamage = 0;

        for (int i = 0; i < 3; i++)
        {
            var (result, dmg) = target.TakeDamage(baseDmg / 2, true, false, false, skill);
            totalDamage += dmg;
        }

        Effect shadowEffect = user.ActiveEffects.Find(s => s.type == EffectType.Shadow);
        if (shadowEffect != null)
        {
            target.AddEffect(new Effect(EffectType.DefenceDown, 2, true, (shadowEffect.intensity * 2)));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} empowerded the droplets with shadow, giving {target.CharacterName} 2 Defence Down.");
        }

        return ($"{user.CharacterName} splashes droplets, striking {target.CharacterName} three times for a total of {totalDamage} damage!");

    }
}
