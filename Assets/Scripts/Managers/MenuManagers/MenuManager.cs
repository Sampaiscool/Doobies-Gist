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
    public GameObject InfoPanel;
    public GameObject AddXPPanel;
    public GameObject DoobieSelectionPanel;

    private GameObject currentPanel;
    private TeamLoader loader; // Referentie naar je JSON systeem

    void Start()
    {
        loader = FindFirstObjectByType<TeamLoader>();
        ShowPanel(Beginpanel);
    }

    public void ShowPanel(GameObject panelToShow)
    {
        // Als we naar het InfoPanel gaan, verversen we eerst de data uit de JSON
        if (panelToShow == InfoPanel)
        {
            UpdateInfoDisplay();
        }

        if (currentPanel != null && currentPanel != panelToShow)
        {
            var animOut = currentPanel.GetComponent<PanelAnimator>();
            if (animOut != null)
                animOut.FadeOut();
            else
                currentPanel.SetActive(false);
        }

        panelToShow.SetActive(true); // Altijd eerst aanzetten voor de animator
        var animIn = panelToShow.GetComponent<PanelAnimator>();
        if (animIn != null)
            animIn.FadeIn();

        currentPanel = panelToShow;
    }

    // Update de teksten in je InfoPanel (Mastery, XP, etc.)
    private void UpdateInfoDisplay()
    {
        GameManager.Instance.FindManagers();
        GameManager.Instance.InfoManager.UpdateInfoPanel();
        Debug.Log("Updating Info Panel with JSON data...");
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
            // Update ook de actuele GameManager stats voor de huidige sessie
            GameManager.Instance.ChangeSploont(999999, true);
            GameManager.Instance.ChangeHp(999, true, true);
        }
    }

    public void OpenDoobies()
    {
        // Laat de lijst met alle doobies vullen
        GameManager.Instance.FindManagers();
        GameManager.Instance.TeamSelectUI.OpenDoobieSelection(); 
        ShowPanel(DoobieSelectionPanel);
    }

    public void OpenXPPanel(bool isForDoobie)
    {
        if (isForDoobie)
        {
            GameManager.Instance.InfoManager.hasChosenDoobie = true;
        }
        else
        {
            GameManager.Instance.InfoManager.hasChosenDoobie = false;
        }
        
        GameManager.Instance.FindManagers();
        GameManager.Instance.InfoManager.UpdateInfoPanel();
        
        ShowPanel(AddXPPanel);
    }

    public void StartGame()
    {
        // VERVANGEN: TeamSelectUI.Instance.SaveTeamData(); (JSON doet dit al bij selectie)
        
        // Zorg dat de loader de juiste Doobie doorgeeft aan de GameManager voor de run
        loader.InitializeSelectedDoobie();
        
        // Geef een start-bonus
        GameManager.Instance.ChangeSploont(150, true);
        
        // Sla de laatste staat van de speler op voordat de scene switcht
        loader.SaveGame();
        
        SceneManager.LoadScene("AdventureScene");
    }
}