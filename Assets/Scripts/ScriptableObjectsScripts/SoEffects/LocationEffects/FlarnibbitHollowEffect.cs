using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Locations/FlarnibbitHollowEffect")]
public class FlarnibbitHollowEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.ChangeHp(5, true, false);

        GameManager.Instance.currentDoobie.MaxHealth += 5;
    }
}
