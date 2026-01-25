using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddButton : MonoBehaviour
{
    public int teamSlotIndex;

    public void OnAddClicked()
    {
        TeamSelectUI.Instance.OpenDoobieSelection(teamSlotIndex);
    }
}
