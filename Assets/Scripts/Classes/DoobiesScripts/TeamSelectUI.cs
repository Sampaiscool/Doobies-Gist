using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectUI : MonoBehaviour
{
    [Header("Selectie Lijst")]
    public Transform doobieHolder; // De grid waar alle 7 doobies in komen
    public GameObject doobieButtonPrefab;

    [Header("Het Paneel")]
    public GameObject doobieSelectionPanel;

    [Header("Referenties")]
    public static TeamSelectUI Instance;
    public MenuManager menuManager;
    private TeamLoader loader;

    private DoobieSO selectedDoobie;

    void Start()
    {
        loader = FindFirstObjectByType<TeamLoader>();
        
        if (loader == null) {
            Debug.LogError("TEAMLOADER NIET GEVONDEN! Staat het TeamLoader script wel in de scene?");
            return;
        }

        LoadTeamDataFromJSON(); 
    }


    // Deze functie vult de lijst met de 7 knoppen
    public void LoadDoobies()
    {
        if (doobieHolder == null || doobieButtonPrefab == null) {
            Debug.LogError("DoobieHolder of Prefab mist in Inspector van TeamSelectUI!");
            return;
        }

        // Haal alles op uit Assets/Resources/Doobies
        DoobieSO[] allDoobies = Resources.LoadAll<DoobieSO>("Doobies");
        Debug.Log($"[DEBUG] Resources gevonden: {allDoobies.Length}");

        // Maak de lijst leeg
        foreach (Transform child in doobieHolder)
            Destroy(child.gameObject);

        foreach (DoobieSO doobie in allDoobies)
        {
            // Check of hij unlocked is
            bool isUnlocked = (GameManager.Instance != null && GameManager.Instance.debugMode) || 
                              doobie.unlockedByDefault || 
                              (loader.data.doobieProgressList.Exists(x => x.doobieName == doobie.doobieName && x.isUnlocked));

            if (isUnlocked)
            {
                GameObject buttonObj = Instantiate(doobieButtonPrefab, doobieHolder);
                DoobieButton buttonScript = buttonObj.GetComponent<DoobieButton>();
                
                // Belangrijk: Setup de knop als een gewone selectieknop (false)
                buttonScript?.SetupButton(doobie, false); 
            }
        }
    }

    // Wordt aangeroepen vanuit MenuManager.cs
    public void OpenDoobieSelection()
    {
        if (doobieSelectionPanel != null)
        {
            doobieSelectionPanel.SetActive(true);
            LoadDoobies();
        }
        else
        {
            Debug.LogError("doobieSelectionPanel is niet gesleept in de Inspector!");
        }
    }

    public void OnDoobieSelected(DoobieSO doobie)
    {
        // 1. Update de lokale variabele
        selectedDoobie = doobie;
    
        // 2. Update de data in de loader
        loader.UpdateSelectedDoobie(doobie.doobieName);
    
        // 3. Sla op
        loader.SaveGame(); 

        Debug.Log($"Data opgeslagen! Geselecteerde Doobie is nu: {doobie.doobieName}");
    
        // 4. UI afhandeling
        if (menuManager != null) menuManager.ShowPanel(menuManager.TownPanel);
    }

    public void LoadTeamDataFromJSON()
    {
        if (loader != null && loader.data != null && !string.IsNullOrEmpty(loader.data.selectedDoobieName))
        {
            selectedDoobie = Resources.Load<DoobieSO>($"Doobies/{loader.data.selectedDoobieName}");
        }
        else
        {
            selectedDoobie = null;
            Debug.Log("Geen geselecteerde Doobie gevonden in JSON (Eerste start?)");
        }
    }
}