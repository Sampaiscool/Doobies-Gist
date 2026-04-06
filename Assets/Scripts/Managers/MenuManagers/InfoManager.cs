using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoManager : MonoBehaviour
{
    [Header("Settings")]
    public int XP_PER_MASTERY = 100; // Hoeveel XP nodig is voor 1 level-up

    [Header("Player UI Elements")]
    public TMP_Text playerFullNameText; // Voor "Title + PlayerName"
    public Image playerPortrait;
    public TMP_Text playerXPText;       // Toont "20 / 100"
    public TMP_Text playerMasteryText;  // Toont "Mastery: 5"

    [Header("Selected Doobie UI Elements")]
    public TMP_Text doobieFullNameText; // Voor "Title + DoobieName"
    public Image doobiePortrait;
    public TMP_Text doobieXPText;       // Toont "20 / 100"
    public TMP_Text doobieMasteryText;  // Toont "Mastery: 5"
    
    private TeamLoader loader;

    void OnEnable()
    {
        loader = FindFirstObjectByType<TeamLoader>();
        if (loader != null)
        {
            UpdateInfoPanel();
        }
    }

    public void UpdateInfoPanel()
    {
        PlayerData data = loader.data;

        if (data == null)
        {
            return;
        }

        // --- 1. Algemene Speler Data ---
        // We voegen de titel en de naam samen (bijv. "Legendary Sampa")
        playerFullNameText.text = $"{data.title} Player"; 
        playerMasteryText.text = $"Mastery: {data.playerMastery}";
        
        // De 20/100 logica voor de speler
        playerXPText.text = $"XP: {data.playerXP} / {XP_PER_MASTERY}";

        // --- 2. Geselecteerde Doobie Data ---
        if (!string.IsNullOrEmpty(data.selectedDoobieName))
        {
            // Zoek de specifieke progressie van deze Doobie
            DoobieProgress progress = data.doobieProgressList.Find(x => x.doobieName == data.selectedDoobieName);

            if (progress != null)
            {
                // De "Flex" naam: combineer Titel + Naam
                doobieFullNameText.text = $"{progress.currentTitle} {data.selectedDoobieName}";
                doobieMasteryText.text = $"Mastery: {progress.doobieMastery}";
                
                // De 20/100 logica voor de Doobie
                doobieXPText.text = $"XP: {progress.doobieXP} / {XP_PER_MASTERY}";
            }
            else
            {
                // Fallback als er nog geen progress is opgeslagen
                doobieFullNameText.text = $"Novice {data.selectedDoobieName}";
                doobieMasteryText.text = "Mastery: 0";
                doobieXPText.text = $"XP: 0 / {XP_PER_MASTERY}";
            }

            // --- 3. Doobie Image inladen ---
            DoobieSO so = Resources.Load<DoobieSO>($"Doobies/{data.selectedDoobieName}");
            if (so != null && doobiePortrait != null)
            {
                doobiePortrait.sprite = so.portrait;
            }
        }
        else
        {
            doobieFullNameText.text = "No Doobie Selected";
            doobieXPText.text = "-";
            doobieMasteryText.text = "-";
        }
    }
}