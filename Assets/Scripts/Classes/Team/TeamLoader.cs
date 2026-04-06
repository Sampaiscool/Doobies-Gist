using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TeamLoader : MonoBehaviour
{
    public PlayerData data;
    private string savePath;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "doobies_save.json");
        LoadGame();
    }
    
    /// <summary>
    /// Saves any changes made to the JSON
    /// </summary>
    public void SaveGame()
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }
    
    public void UpdateSelectedDoobie(string newName)
    {
        data.selectedDoobieName = newName;

        if (newName != "")
        {
            SaveGame();
            Debug.Log("JSON geüpdatet met nieuwe Doobie: " + newName);
        }
    }

    public void LoadGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            data = JsonUtility.FromJson<PlayerData>(json);

            // EXTRA CHECK: Als de file bestond maar de lijst was leeg/corrupt
            if (data.doobieProgressList == null) 
                data.doobieProgressList = new List<DoobieProgress>();
        }
        else
        {
            data = new PlayerData(); 
            data.doobieProgressList = new List<DoobieProgress>();
            data.selectedDoobieName = "";
        
            SaveGame();
            Debug.Log("Nieuwe savefile aangemaakt met lege lijsten.");
        }
    }

    public void AddSavedXP(int amount)
    {
        data.savedXP += amount;
        SaveGame();
    }
    
    public void AddExperienceTo(string name, int xpAmount, bool toDoobie)
    {
        GameManager.Instance.FindManagers();
        int requiredXP = GameManager.Instance.InfoManager.XP_PER_MASTERY;
        
        if (toDoobie)
        {
            DoobieProgress progress = data.doobieProgressList.Find(x => x.doobieName == name);

            if (progress == null)
            {
                progress = new DoobieProgress();
                progress.doobieName = name;
                progress.currentTitle = Titles.Rookie;
                progress.isUnlocked = true;
                data.doobieProgressList.Add(progress);
            }
            progress.doobieXP += xpAmount;

            while (progress.doobieXP >= requiredXP)
            {
                progress.doobieXP -= requiredXP;
                progress.doobieMastery++;
            }
        }
        else
        {
            data.playerXP += xpAmount;

            while (data.playerXP >= requiredXP)
            {
                data.playerXP -= requiredXP;
                data.playerMastery++;
            }
        }
        
        SaveGame();
    }

    public void InitializeSelectedDoobie()
    {
        if (string.IsNullOrEmpty(data.selectedDoobieName)) return;

        DoobieSO so = Resources.Load<DoobieSO>($"Doobies/{data.selectedDoobieName}");
        if (so != null)
        {
            GameManager.Instance.currentDoobie = new DoobieInstance(so);
            Debug.Log($"Loaded {data.selectedDoobieName} from JSON!");
        }
    }
}