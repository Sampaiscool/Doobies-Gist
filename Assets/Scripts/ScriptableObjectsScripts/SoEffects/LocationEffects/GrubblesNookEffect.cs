using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/GrubblesNookEffect")]
public class GrubblesNookEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        var combatant = GameManager.Instance.currentDoobie; // This is a CombatantInstance reference

        if (combatant is DoobieInstance doobie)
        {
            combatant.EquippedWeaponInstance.bonusDamage += 2;
            Debug.Log("Gained +2 damage");
        }
        else
        {
            Debug.LogWarning("GrubblesNookEffect can only be applied to a DoobieInstance.");
        }
    }
}
