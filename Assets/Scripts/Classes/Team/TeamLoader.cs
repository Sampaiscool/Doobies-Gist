using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamLoader : MonoBehaviour
{
    private DoobieSO selectedDoobie;

    // Start is called before the first frame update
    void Start()
    {
        LoadTeamData();  // Load team data from PlayerPrefs
        LogSelectedDoobie();  // Log the saved team to the console
    }

    // Load team data from PlayerPrefs
    public void LoadTeamData()
    {
        string name = PlayerPrefs.GetString("SelectedDoobie_Name", "");
        if (!string.IsNullOrEmpty(name))
        {
            selectedDoobie = Resources.Load<DoobieSO>($"Doobies/{name}");

            if (selectedDoobie != null)
            {
                // The Chosen One awakens!
                GameManager.Instance.currentDoobie = new DoobieInstance(selectedDoobie);
            }
            else
            {
                Debug.LogWarning($"Doobie '{name}' not found in Resources/Doobies!");
            }
        }
    }

    // Log the saved team in the console
    void LogSelectedDoobie()
    {
        if (selectedDoobie == null)
        {
            Debug.Log("No Doobie selected.");
            return;
        }

        string log = $" Selected Doobie:\n" +
                     $"- Name: {selectedDoobie.doobieName}\n" +
                     $"- Zurp (Mana): {selectedDoobie.zurp}\n" +
                     $"- Base Health: {selectedDoobie.baseHealth}\n" +
                     $"- Has Health?: {selectedDoobie.hasHealth}\n" +
                     $"- Default Weapon: {(selectedDoobie.defaultWeapon != null ? selectedDoobie.defaultWeapon.weaponName : "None")}\n";

        log += "- Base Skills:\n";
        if (selectedDoobie.baseSkills != null && selectedDoobie.baseSkills.Count > 0)
        {
            foreach (SkillSO skill in selectedDoobie.baseSkills)
            {
                log += $"  • {skill.skillName}\n";
            }
        }
        else
        {
            log += "  • None\n";
        }

        Debug.Log(log);
    }
}
