using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject Beginpanel;
    public GameObject StartPanel;
    public GameObject TownPanel;
    public GameObject TeamPanel;
    public GameObject DoobieSelectionPanel;

    private GameObject currentPanel;

    void Start()
    {
        ShowPanel(Beginpanel);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        if (currentPanel != null && currentPanel != panelToShow)
        {
            var animOut = currentPanel.GetComponent<PanelAnimator>();
            if (animOut != null)
                animOut.FadeOut();
            else
                currentPanel.SetActive(false);
        }

        var animIn = panelToShow.GetComponent<PanelAnimator>();
        if (animIn != null)
            animIn.FadeIn();
        else
            panelToShow.SetActive(true);

        currentPanel = panelToShow;
    }

    // Button hooks
    public void OnStartButtonClicked() => ShowPanel(StartPanel);

    public void OnGoClicked() => ShowPanel(TownPanel);

    public void EnableDebug()
    {
        GameManager.Instance.debugMode = !GameManager.Instance.debugMode;
        Debug.Log("Debug Mode: " + GameManager.Instance.debugMode);
        if (GameManager.Instance.debugMode)
        {
            GameManager.Instance.ChangeSploont(999999, true);
            GameManager.Instance.ChangeHp(99999, true, true);
        }
    }

    public void OpenDoobies()
    {
        TeamSelectUI.Instance.UpdateTeamUI();
        ShowPanel(DoobieSelectionPanel);
    }

    public void StartGame()
    {
        TeamSelectUI.Instance.SaveTeamData();
        SceneManager.LoadScene("AdventureScene");
    }
}
