using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Locations/ThePlonkthropEffect")]
public class ThePlonkthropEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.ChangeHp(10, true, false);
    }
}
