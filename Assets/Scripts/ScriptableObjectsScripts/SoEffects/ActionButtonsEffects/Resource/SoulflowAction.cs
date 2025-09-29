using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/ResourceActions/SoulflowAction")]
public class SoulflowAction : ScriptableObject, IResourceAction
{
    public string ActionName => "Soulflow sense";
    public string Description => "Lose 2Energy of the current transformation you're in; gain 2 energy of the other transformation";
    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
        {
            if (user.CurrentTransformation == Transformations.WorldForm)
            {
                soulflow.WorldEnergy.Spend(2);
                soulflow.SpiritEnergy.Gain(2);
            }
            else if (user.CurrentTransformation == Transformations.SpiritForm)
            {
                soulflow.WorldEnergy.Gain(2);
                soulflow.SpiritEnergy.Spend(2);
            }
        }
        
        return true;
    }
}
