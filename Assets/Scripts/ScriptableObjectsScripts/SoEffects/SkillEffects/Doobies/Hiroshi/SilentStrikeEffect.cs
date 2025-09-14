using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Hiroshi/SilentStrike")]
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
        // Get the base damage (weapon or spell-defined)
        int baseDamage = user.GetEffectiveWeaponDamage();

        // Apply buffs/debuffs that modify outgoing damage
        int finalDamage = user.GetEffectiveDamageAfterBuffs(baseDamage);

        // Deal damage
        int targetBefore = target.CurrentHealth;
        target.TakeDamage(finalDamage, isSkill: true);
        int actualDamage = targetBefore - target.CurrentHealth;

        // Heal the user for the same amount
        user.CurrentHealth += actualDamage;

        return $"{user.CharacterName} strikes silently for {actualDamage} damage and heals for the same.";
    }
}