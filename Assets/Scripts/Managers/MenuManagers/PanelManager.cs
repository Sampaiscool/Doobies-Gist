using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManager : MonoBehaviour
{
    public GameObject TutorialPanel;
    public GameObject LocationPanel;
    public GameObject VangurrPanel;
    public LocationManager locationManager;
    public VangurrManager vangurrManager;

    void Start()
    {
        if (GameManager.Instance.HasDoneTutorial)
        {
            ShowLocationPanel();
        }
        else
        {
            ShowPanel(TutorialPanel);
            GameManager.Instance.HasDoneTutorial = true;
        }
    }

    public void ShowPanel(GameObject panelToShow)
    {
        // Deactivate all known panels
        TutorialPanel.SetActive(false);
        LocationPanel.SetActive(false);
        VangurrPanel.SetActive(false);

        // Activate the chosen one
        panelToShow.SetActive(true);
    }

    public void ShowLocationPanel()
    {
        ShowPanel(LocationPanel);

        // Generate random locations and display them in the panel
        locationManager.GenerateRandomLocations(3);  // 3 random locations
    }

    public void ShowVangurrPanel()
    {
        ShowPanel(VangurrPanel);

        // Choose a random Vangurr based on difficulty
        VangurrSO selectedVangurr = vangurrManager.ChooseVangurr();

        // Create a new VangurrInstance and assign it
        GameManager.Instance.currentVangurr = new VangurrInstance(selectedVangurr);

        // Update the text
        vangurrManager.UpdateVangurrText(selectedVangurr);
    }

    public void OnBattlePressed()
    {
        SceneManager.LoadScene("BattleScene");
    }
}
