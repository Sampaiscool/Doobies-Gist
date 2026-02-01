using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/MightyFire/RagingFlameEffect")]
public class RagingFlameEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        target.AddEffect(new Effect(EffectType.VanishedDefense, 3, true, 1));

        int damage = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg * 4));

        var (result, damageDone) = target.TakeDamage(damage, true, false, false, skill);

        int opponentBurn = target.GetTotalEffectIntensity(EffectGroup.BurnLike);
        
        user.AddEffect(new Effect(EffectType.Regeneration, 2, false, (opponentBurn / 2)));
        
        return $"{user.CharacterName} flame went right trough {target.CharacterName}'s defence, dealing {damageDone} damage, giving {user.CharacterName} regeneration in the process!";
    }
}
