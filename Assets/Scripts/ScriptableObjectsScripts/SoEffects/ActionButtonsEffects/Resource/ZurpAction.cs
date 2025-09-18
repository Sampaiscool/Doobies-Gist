using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/ResourceActions/ZurpAction")]
public class ZurpAction : ScriptableObject, IResourceAction
{
    public string ActionName => "Focus";
    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        if (user is DoobieInstance doobie && doobie.MainResource is ZurpResource zurp)
        {
            if (doobie.MainResource.Current != doobie.MainResource.Max)
            {
                doobie.MainResource.Gain(2);
                BattleUIManager.Instance.AddLog($"{user.CharacterName} Gains some zurp.");
                return true;
            }
            else
            {
                BattleUIManager.Instance.AddLog($"{user.CharacterName} already has max zurp!");
                return false;
            }
        }
        else
        {
            BattleUIManager.Instance.AddLog($"Something went wrong with the mainresource of {user.CharacterName}");
            return false;
        } 
    }
}
