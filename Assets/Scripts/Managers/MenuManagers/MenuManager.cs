using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public GameObject Beginpanel;
    public GameObject StartPanel;
    public GameObject TownPanel;
    public GameObject TeamPanel;            // shows your current team
    public GameObject DoobieSelectionPanel; // shows all available doobies

    void Start()
    {
        ShowPanel(Beginpanel);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        // Deactivate all known panels
        Beginpanel.SetActive(false);
        StartPanel.SetActive(false);
        TownPanel.SetActive(false);
        TeamPanel.SetActive(false);
        DoobieSelectionPanel.SetActive(false);

        // Activate the chosen one
        panelToShow.SetActive(true);
    }

    // These can be hooked up to buttons
    public void OnStartButtonClicked()
    {
        ShowPanel(StartPanel);
    }

    public void OnGoClicked()
    {
        ShowPanel(TownPanel);
    }

    public void EnableDebug()
    {
        GameManager.Instance.debugMode = !GameManager.Instance.debugMode;
        Debug.Log("Debug Mode: " + GameManager.Instance.debugMode);
    }

    public void OpenDoobies()
    {
        TeamSelectUI.Instance.UpdateTeamUI();
        ShowPanel(DoobieSelectionPanel);
    }

    public void StartGame()
    {
        TeamSelectUI.Instance.SaveTeamData(); // Save the selected team data
        SceneManager.LoadScene("AdventureScene");
    }
}
