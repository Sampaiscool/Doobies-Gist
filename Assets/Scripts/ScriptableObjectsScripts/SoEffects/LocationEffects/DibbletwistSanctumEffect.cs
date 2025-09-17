using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/DibbletwistSanctumEffect")]
public class DibbletwistSanctumEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        int currentHP = GameManager.Instance.currentDoobie.CurrentHealth;

        currentHP = currentHP/ 2;

        GameManager.Instance.currentDoobie.CurrentHealth = currentHP;

        GameManager.Instance.ChangeSploont(100, true);

        GameManager.Instance.currentDoobie.EquippedWeaponInstance.bonusDamage += 5;

        GameManager.Instance.currentDoobie.CurrentSkillDmg += 5;

        GameManager.Instance.currentDoobie.CurrentHealPower += 2;
    }
}
