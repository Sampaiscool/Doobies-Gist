using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/SploontBazaarEffect")]
public class SploontBazaarEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.ChangeSploont(300, true);
    }
}
