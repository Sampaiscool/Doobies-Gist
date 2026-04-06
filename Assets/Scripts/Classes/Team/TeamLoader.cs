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
            SaveGame(); // Schrijf direct naar de .json file
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
            // EERSTE KEER OOIT:
            data = new PlayerData(); 
            data.doobieProgressList = new List<DoobieProgress>();
            data.selectedDoobieName = "";
        
            SaveGame();
            Debug.Log("Nieuwe savefile aangemaakt met lege lijsten.");
        }
    }
    
    public void AddExperienceToDoobie(string name, int xpAmount)
    {
        // 1. Zoek of deze Doobie al in de lijst staat
        DoobieProgress progress = data.doobieProgressList.Find(x => x.doobieName == name);

        // 2. Als hij nog niet bestaat (eerste keer mee gespeeld), maak hem aan
        if (progress == null)
        {
            progress = new DoobieProgress();
            progress.doobieName = name;
            progress.currentTitle = Titles.Rookie;
            progress.isUnlocked = true;
            data.doobieProgressList.Add(progress);
        }

        // 3. Voeg XP toe
        progress.doobieXP += xpAmount;

        // 4. Check voor Mastery Level Up (De 20/100 logica)
        while (progress.doobieXP >= 100)
        {
            progress.doobieXP -= 100;
            progress.doobieMastery++;
            Debug.Log($"{name} is nu Mastery Level {progress.doobieMastery}!");
        
            // Hier kun je eventueel titels checken:
            // if(progress.doobieMastery == 10) progress.currentTitle = Titles.Expert;
        }

        // 5. Direct opslaan in de JSON
        SaveGame();
    }

    // Deze methode vervangt je oude LoadTeamData
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