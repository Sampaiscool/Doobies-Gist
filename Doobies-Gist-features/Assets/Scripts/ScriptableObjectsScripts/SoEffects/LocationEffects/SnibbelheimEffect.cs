using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/SnibbelheimEffect")]
public class SnibbelheimEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusCritChance += 50;
        if (GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusCritChance > 100)
        {
            GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusCritChance = 100;
        }

        GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusDamage += 20;

        GameManager.Instance.currentDoobie.CurrentSkillDmg += 20;
    }
}
