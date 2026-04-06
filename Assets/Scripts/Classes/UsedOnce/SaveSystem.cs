using UnityEngine;
using System.IO;

public static class SaveSystem
{
    private static string savePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

    public static void Save(PlayerData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log("Game Saved to: " + savePath);
    }

    public static PlayerData Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<PlayerData>(json);
        }
        return new PlayerData();
    }
}
