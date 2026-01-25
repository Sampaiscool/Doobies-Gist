using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/VeiledPlaceEffect")]
public class VeiledPlaceEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.ChangeHp(1, true, true);
    }
}
