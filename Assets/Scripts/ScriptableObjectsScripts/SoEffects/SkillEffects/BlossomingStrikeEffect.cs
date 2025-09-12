using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/BlossomingStrike")]
public class BlossomingStrikeEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDamage = GetBaseDamageFromSO(user);
        int blossomDamage = Mathf.RoundToInt(baseDamage * 0.5f);

        target.TakeDamage(blossomDamage, isSkill: true);
        user.DeflectNextAttack = true;

        return $"{user.CharacterName} dances forward with a Blossoming Strike, dealing {blossomDamage} damage and readying to deflect the next attack!";
    }

    private int GetBaseDamageFromSO(CombatantInstance user)
    {
        if (user == null)
        {
            Debug.LogWarning("CombatantInstance user is null.");
            return 0;
        }

        return user.GetEffectiveWeaponDamage();
    }
}
