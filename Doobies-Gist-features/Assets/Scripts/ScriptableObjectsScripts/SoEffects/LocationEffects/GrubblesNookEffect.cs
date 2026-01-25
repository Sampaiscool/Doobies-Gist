using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/GrubblesNookEffect")]
public class GrubblesNookEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.ChangeHp(5, false, false);

        GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusDamage += 2;
    }
}
