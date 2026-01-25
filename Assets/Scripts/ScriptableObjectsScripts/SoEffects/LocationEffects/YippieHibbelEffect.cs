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
            case ResourceType.Rum:
                GameManager.Instance.currentDoobie.MainResource.GainMax(5);
                break;
            case ResourceType.Faith:
                GameManager.Instance.currentDoobie.MainResource.GainMax(5);
                break;
            case ResourceType.Soulflow:
                if (GameManager.Instance.currentDoobie.MainResource is SoulflowResource souflow)
                {
                    souflow.WorldEnergy.GainMax(5);
                    souflow.SpiritEnergy.GainMax(5);
                }
                break;
            case ResourceType.Crystals:
                GameManager.Instance.currentDoobie.MainResource.GainMax(2);
                break;
            default:
                break;
        }
        
    }
}
