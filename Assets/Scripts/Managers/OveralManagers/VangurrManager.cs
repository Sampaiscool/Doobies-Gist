using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VangurrManager : MonoBehaviour
{
    public TMP_Text VangurrTextObject;
    public List<VangurrSO> VangurrList = new List<VangurrSO>();
    public VangurrSO ChosenVangurr;
    // Start is called before the first frame update
    void Start()
    {

    }

    /// <summary>
    /// Chooses a random vangurr based on difficulty
    /// </summary>
    /// <returns>The selected vangurr</returns>
    public VangurrSO ChooseVangurr()
    {
        int currentDifficulty = GameManager.Instance.CurrentDifficulty;

        // Filter de lijst op moeilijkheidsgraad
        List<VangurrSO> filteredList = VangurrList.FindAll(v => v.difficultyLevel == currentDifficulty);

        if (filteredList.Count == 0)
        {
            Debug.LogWarning("Geen Vangurrs gevonden voor difficulty " + currentDifficulty);
            return null;
        }

        int chosenIndex = Random.Range(0, filteredList.Count);
        VangurrSO chosen = filteredList[chosenIndex];

        ChosenVangurr = chosen;
        return chosen;
    }

    /// <summary>
    /// Updates the text based on the Vangurss value
    /// </summary>
    /// <param name="selectedVangurr">The selected vangurr</param>
    public void UpdateVangurrText(VangurrSO selectedVangurr)
    {
        if (selectedVangurr != null)
        {
            VangurrTextObject.text = selectedVangurr.VangurrText;
        }
        else
        {
            VangurrTextObject.text = "Geen Vangurr gevonden...";
        }
    }

    public void OnBattleClick()
    {

    }
}

