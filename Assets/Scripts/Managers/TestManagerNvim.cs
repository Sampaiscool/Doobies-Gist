using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestManagerNvim : MonoBehaviour
{
    public GameObject IAmNvim;

    public void OnClickOfNvim()
    {
        Debug.Log("You have been clicked!");

        BattleUIManager.Instance.AddLog($"miaw");
    }
    
    // Ik ben een unsaved change! nogmaals want ik ben dom
    // Dit is een coole test! :)
}
