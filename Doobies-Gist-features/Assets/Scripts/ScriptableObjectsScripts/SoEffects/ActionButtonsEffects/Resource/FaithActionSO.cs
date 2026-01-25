using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/ResourceActions/FaithAction")]
public class FaithActionSO : ScriptableObject, IResourceAction
{
    public string ActionName => "Swap Goddess";
    public string Description => "Swap your goddess";
    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        if (user is DoobieInstance doobie && doobie.MainResource is FaithResource faith)
        {
            GameManager.Instance.SpawnGoddessButtons();
        }
        else
        {
            BattleUIManager.Instance.AddLog($"Something went wrong with the mainresource of {user.CharacterName}");
            
        }
        return false;
    }
}
