using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Serialization;

public class InfoManager : MonoBehaviour
{
    [FormerlySerializedAs("XP_PER_MASTERY")] [Header("Settings")]
    public int xpPerMastery;

    public bool hasChosenDoobie;

    [Header("Global Elements")] 
    public TMP_Text savedXPText;

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

    [Header("Add XP Panel Elements")]
    public TMP_Text savedXPText2;
    public TMP_Text chosenNameText;
    public Image chosenPortrait;
    public TMP_Text chosenXPText;
    public TMP_Text chosenMasteryText;
    public TMP_InputField xpAmountInput;
    public Button confirmAddXPButton;
    
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

        savedXPText.text = $"{data.savedXP} Saved XP";
        
        playerFullNameText.text = $"{data.title} Player"; 
        playerMasteryText.text = $"Mastery: {data.playerMastery}";
        
        playerXPText.text = $"XP: {data.playerXP} / {xpPerMastery}";

        if (!string.IsNullOrEmpty(data.selectedDoobieName))
        {
            DoobieProgress progress = data.doobieProgressList.Find(x => x.doobieName == data.selectedDoobieName);

            if (progress != null)
            {
                doobieFullNameText.text = $"{progress.currentTitle} {data.selectedDoobieName}";
                doobieMasteryText.text = $"Mastery: {progress.doobieMastery}";
                
                doobieXPText.text = $"XP: {progress.doobieXP} / {xpPerMastery}";
            }
            else
            {
                doobieFullNameText.text = $"Novice {data.selectedDoobieName}";
                doobieMasteryText.text = "Mastery: 0";
                doobieXPText.text = $"XP: 0 / {xpPerMastery}";
            }

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

        savedXPText2.text = savedXPText.text;
        
        if (hasChosenDoobie)
        {
            chosenNameText.text = doobieFullNameText.text;
            chosenPortrait.sprite = doobiePortrait.sprite;
            chosenXPText.text = doobieXPText.text;
            chosenMasteryText.text = doobieMasteryText.text;
        }
        else
        {
            chosenNameText.text = playerFullNameText.text;
            chosenPortrait.sprite = playerPortrait.sprite;
            chosenXPText.text = playerXPText.text;
            chosenMasteryText.text = playerMasteryText.text;
        }
    }
    
    public void OnAddXPButtonClicked()
    {
        if (string.IsNullOrEmpty(xpAmountInput.text)) return;

        if (int.TryParse(xpAmountInput.text, out int amountToSpend))
        {
            PlayerData data = loader.data;

            if (amountToSpend > data.savedXP)
            {
                Debug.LogWarning("Niet genoeg Saved XP!");
                
                return; 
            }

            data.savedXP -= amountToSpend;

            if (hasChosenDoobie)
            {
                loader.AddExperienceTo(data.selectedDoobieName, amountToSpend, true);
            }
            else
            {
                loader.AddExperienceTo("Player", amountToSpend, false);
            }

            UpdateInfoPanel();
            xpAmountInput.text = "";
        }
    }
}