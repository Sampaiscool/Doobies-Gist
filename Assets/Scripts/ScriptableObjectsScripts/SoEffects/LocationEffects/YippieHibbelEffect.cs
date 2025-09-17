using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/YippieHibbelEffect")]
public class YippieHibbelEffect : LocationEffectSO
{
    public override void ApplyEffect()
    {
        var currentResource = GameManager.Instance.currentDoobie.MainResource.Type;
        switch (currentResource)
        {
            case ResourceType.Zurp:
                GameManager.Instance.currentDoobie.MainResource.GainMax(2);
                break;
            case ResourceType.Health:
                GameManager.Instance.currentDoobie.MaxHealth += 5;
                GameManager.Instance.currentDoobie.CurrentHealth += 5;
                break;
            default:
                break;
        }
        
    }
}
