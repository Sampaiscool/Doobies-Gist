using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/DibbletwistSanctumEffect")]
public class DibbletwistSanctumEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        GameManager.Instance.CurrentDifficulty += 1;

        GameManager.Instance.ChangeSploont(2000, true);

        GameManager.Instance.ChangeHp(10, true, true);
    }
}
