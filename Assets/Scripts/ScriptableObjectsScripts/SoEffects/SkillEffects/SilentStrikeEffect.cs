using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/SilentStrike")]
public class SilentStrikeEffect : SkillEffectSO
{
    /// <summary>
    /// Stab your opponent and heal for damage dealt
    /// </summary>
    /// <param name="user">The instance that used the skill</param>
    /// <param name="target">The instance that is targeted</param>
    /// <returns></returns>
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        // Check if user.so is a DoobieSO or VangurrSO
        int damage = 0;

        damage = user.GetEffectiveWeaponDamage();

        int targetBefore = target.CurrentHealth;

        target.TakeDamage(damage);
        int actualDamage = targetBefore - target.CurrentHealth;

        user.CurrentHealth += actualDamage;

        return $"{user.CharacterName} strikes silently for {actualDamage} damage and heals for the same.";
    }
}