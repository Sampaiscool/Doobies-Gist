using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DoobieButton : MonoBehaviour
{
    public DoobieSO doobieData;
    public Button button;
    public TMP_Text nameText;
    public Image image;

    private bool isTeamSlot;
    private int teamSlotIndex;

    //private bool isForTeamSlot = false;

    // Initialize button with the DoobieSO data
    public void SetupButton(DoobieSO doobie, bool isTeamSlot = false, int teamSlotIndex = -1)
    {
        doobieData = doobie;
        this.isTeamSlot = isTeamSlot;
        this.teamSlotIndex = teamSlotIndex;

        nameText.text = doobie.doobieName;
        image.sprite = doobie.portrait;
        button.onClick.RemoveAllListeners();
        if (isTeamSlot)
        {
            button.onClick.AddListener(() =>
            {
                TeamSelectUI.Instance.OpenDoobieSelection(teamSlotIndex);
            });
        }
        else
        {
            button.onClick.AddListener(() =>
            {
                TeamSelectUI.Instance.OnDoobieSelected(doobieData, TeamSelectUI.Instance.selectedSlot);
            });
        }
    }
}
