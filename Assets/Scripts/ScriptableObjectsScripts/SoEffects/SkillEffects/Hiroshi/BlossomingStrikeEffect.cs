using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Hiroshi/BlossomingStrike")]
public class BlossomingStrikeEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        if (user == null || target == null)
        {
            Debug.LogWarning("CombatantInstance user or target is null.");
            return "The strike fizzles...";
        }

        // Get base damage from weapon or attack stats
        int baseDamage = user.GetEffectiveWeaponDamage();

        // Apply outgoing damage buffs/debuffs (Weaken, etc.)
        int modifiedDamage = user.GetEffectiveDamageAfterBuffs(baseDamage);

        // Blossom portion: half of modified damage
        int blossomDamage = Mathf.RoundToInt(modifiedDamage * 0.5f);

        // Deal damage to the target
        int targetBefore = target.CurrentHealth;
        target.TakeDamage(blossomDamage, isSkill: true);
        int actualDamage = targetBefore - target.CurrentHealth;

        // Apply Deflection buff to the user
        Buff deflectBuff = new Buff(BuffType.Deflecion, duration: 999, isDebuff: false, 1);
        user.AddBuff(deflectBuff);

        return $"{user.CharacterName} dances forward with a Blossoming Strike, dealing {actualDamage} damage and readying to deflect the next attack!";
    }
}
