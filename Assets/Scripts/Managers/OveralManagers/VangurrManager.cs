using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class VangurrGroup
{
    public string label;           // e.g. "Difficulty 1 Enemies" 
    public int difficulty;         // difficulty this pool belongs to
    public bool isBossGroup;       // true = bosses, false = normal enemies
    public List<VangurrSO> vangurrs = new List<VangurrSO>();
}

public class VangurrManager : MonoBehaviour
{
    public TMP_Text VangurrTextObject;

    [Header("Organized Vangurr Pools")]
    public List<VangurrGroup> vangurrGroups = new List<VangurrGroup>();

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
        bool isBossFight = GameManager.Instance.BattlesFought >= GameManager.Instance.MaxBattlesBeforeBoss;

        // Find the right group
        VangurrGroup group = vangurrGroups.Find(g =>
            g.difficulty == currentDifficulty &&
            g.isBossGroup == isBossFight);

        if (group == null || group.vangurrs.Count == 0)
        {
            Debug.LogWarning($"Geen Vangurrs gevonden voor difficulty {currentDifficulty} (Boss? {isBossFight})");
            return null;
        }

        // Random pick from that group
        int chosenIndex = Random.Range(0, group.vangurrs.Count);
        ChosenVangurr = group.vangurrs[chosenIndex];
        return ChosenVangurr;
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

