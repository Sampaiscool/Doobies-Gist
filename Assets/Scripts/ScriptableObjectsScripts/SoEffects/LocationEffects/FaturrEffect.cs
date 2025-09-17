using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/FaturrEffect")]
public class FaturrEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.ChangeHp(5, false, false);

        GameManager.Instance.currentDoobie.CurrentSkillDmg += 2;
    }
}
