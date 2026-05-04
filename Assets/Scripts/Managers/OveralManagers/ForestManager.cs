using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    public static ForestManager Instance;

    [Header("Tree Growth Per Visit")]
    public int growthPerVisit = 1;

    private TeamLoader loader;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        loader = FindFirstObjectByType<TeamLoader>();
    }

    /// <summary>
    /// Plant een nieuwe Ravin Tree. Kost nu 'Harvest' valuta direct.
    /// </summary>
    public bool PlantTree(bool timeBased)
    {
        var data = loader.data;
        if (data.ravinTreeList.Count >= 3) return false; // MAX 3 BOMEN
    
        int treeCost = 5;
        if (data.harvest < treeCost) return false;

        data.harvest -= treeCost;
        data.ravinTreeList.Add(new RavinTree(timeBased));
        loader.SaveGame();
        return true;
    }

    /// <summary>
    /// Wordt aangeroepen na het verslaan van een baas/vangurr.
    /// </summary>
    public void GrowAllTrees()
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        var data = loader.data;

        foreach (var tree in data.ravinTreeList)
        {
            if (!tree.isComplete)
            {
                tree.growthCurrent += growthPerVisit;
                if (tree.growthCurrent >= tree.growthRequired)
                {
                    tree.growthCurrent = tree.growthRequired;
                    tree.isComplete = true;
                }
            }
        }
        loader.SaveGame();
    }

    /// <summary>
    /// Oogst de boom en geeft een item via de ItemManager.
    /// </summary>
    public bool ClaimTree(string treeId)
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        var data = loader.data;

        var tree = data.ravinTreeList.Find(t => t.treeId == treeId);
        if (tree == null || !tree.isComplete || tree.isClaimed) return false;

        // Genereer item beloning
        if (ItemManager.Instance != null)
        {
            EquippableItem newItem = ItemManager.Instance.GenerateRandomItem();
            ItemManager.Instance.AddToInventory(newItem);
            
            // Markeer als geclaimd of verwijder de boom uit de lijst
            tree.isClaimed = true; 
            // Optioneel: data.ravinTreeList.Remove(tree); als de plek weer vrij moet komen.
            
            loader.SaveGame();
            return true;
        }

        return false;
    }

    public List<RavinTree> GetAllTrees()
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        return loader.data.ravinTreeList;
    }
}