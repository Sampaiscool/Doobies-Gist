using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/MagicSpright/ShadowPulseEffect")]
public class ShadowPulseEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int baseDmg = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg));
        var (result, dmgDone) = target.TakeDamage(baseDmg * 2, true, false, false, skill);

        string log = $"{user.CharacterName} releases a pulse of dark energy, striking {target.CharacterName} for {dmgDone} damage!";

        Effect shadowEffect = user.ActiveEffects.Find(s => s.type == EffectType.Shadow);
        if (shadowEffect != null)
        {
            int intensity = shadowEffect.intensity;

            for (int i = 0; i < intensity; i++)
            {
                var (shadowResult, shadowDmg) = target.TakeDamage(baseDmg / 2, true, false, false, skill);
                log += $" The lingering shadow echoes for {shadowDmg} extra damage!";
            }

            BattleUIManager.Instance.AddLog($"{user.CharacterName}'s shadow energy surged {intensity} times!");
        }

        return log;
    }
}
