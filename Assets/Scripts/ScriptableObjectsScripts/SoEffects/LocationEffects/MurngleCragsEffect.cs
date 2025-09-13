using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/MurngleCragsEffect")]
public class MurngleCragsEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusCritChance += 5;

        if (GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusCritChance > 100)
        {
            GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusCritChance = 100;
        }
    }
}
