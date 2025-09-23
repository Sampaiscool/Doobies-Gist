using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/ResourceActions/RumAction")]
public class RumActionSO : ScriptableObject, IResourceAction
{
    public string ActionName => "Make Rum";
    public string Description => "Gain 1 / 5 Rum\n";
    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        if (user is DoobieInstance doobie && doobie.MainResource is RumResource rum)
        {
            
        }
        return true;
    }
}
