using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/MightyFire/AshBlastEffect")]
public class AshBlastEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        int damage = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg * 2));
        
        var (result, damageDone) = target.TakeDamage(damage, true, false, false, skill);

        if (user.CurrentBurnLevel != 3)
        {
            target.AddEffect(new Effect(EffectType.Burn, 3, true, (damageDone / 2)));
        }
        else
        {
            target.AddEffect(new Effect(EffectType.Burn4, 3, true, (damageDone / 2)));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} Unleashed a final burn level!");
        }

        return $"{user.CharacterName} fired a blast at {target.CharacterName} dealing {damage} damage and inflicting {damageDone / 2} burn!";
    }
}
