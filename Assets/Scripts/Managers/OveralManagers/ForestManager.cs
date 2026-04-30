using System.Collections.Generic;
using UnityEngine;

public class ForestManager : MonoBehaviour
{
    public static ForestManager Instance;

    [Header("Tree Growth Per Visit")]
    public int growthPerVisit = 1;

    [Header("Tree Rewards")]
    public int turipSeedReward = 3;
    public int doobieSeedReward = 2;
    public int playerXPReward = 50;

    private TeamLoader loader;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        loader = FindFirstObjectByType<TeamLoader>();
    }

    /// <summary>
    /// Plant a seed of the given type. Consumes the seed and creates a new RavinTree.
    /// </summary>
    public bool PlantSeed(SeedType seedType)
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        var data = loader.data;

        switch (seedType)
        {
            case SeedType.Turip:
                if (data.turipSeeds <= 0) return false;
                data.turipSeeds--;
                break;
            case SeedType.Doobie:
                if (data.doobieSeeds <= 0) return false;
                data.doobieSeeds--;
                break;
            case SeedType.Player:
                if (data.playerSeeds <= 0) return false;
                data.playerSeeds--;
                break;
        }

        data.ravinTreeList.Add(new RavinTree(seedType));
        loader.SaveGame();
        return true;
    }

    /// <summary>
    /// Visit a tree and grow it. Returns true if the tree is now complete.
    /// </summary>
    public bool VisitTree(string treeId)
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        var data = loader.data;

        var tree = data.ravinTreeList.Find(t => t.treeId == treeId);
        if (tree == null || tree.isComplete) return false;

        tree.growthCurrent += growthPerVisit;

        if (tree.growthCurrent >= tree.growthRequired)
        {
            tree.growthCurrent = tree.growthRequired;
            tree.isComplete = true;
        }

        loader.SaveGame();
        return tree.isComplete;
    }

    /// <summary>
    /// Claim a completed tree's reward. Applies reward based on seed type.
    /// </summary>
    public bool ClaimTree(string treeId)
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        var data = loader.data;

        var tree = data.ravinTreeList.Find(t => t.treeId == treeId);
        if (tree == null || !tree.isComplete || tree.isClaimed) return false;

        tree.isClaimed = true;

        // Apply reward based on seed type
        switch (tree.seedType)
        {
            case SeedType.Turip:
                data.turipSeeds += turipSeedReward;
                break;
            case SeedType.Doobie:
                data.doobieSeeds += doobieSeedReward;
                break;
            case SeedType.Player:
                data.savedXP += playerXPReward;
                break;
        }

        loader.SaveGame();
        return true;
    }

    /// <summary>
    /// Get all trees in the forest.
    /// </summary>
    public List<RavinTree> GetAllTrees()
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        return loader.data.ravinTreeList;
    }

    /// <summary>
    /// Buy seeds using Harvest currency. Returns true if purchase succeeded.
    /// </summary>
    public bool BuySeed(SeedType seedType)
    {
        if (loader == null) loader = FindFirstObjectByType<TeamLoader>();
        var data = loader.data;

        if (data.harvest <= 0) return false;

        data.harvest--;

        switch (seedType)
        {
            case SeedType.Turip:
                data.turipSeeds++;
                break;
            case SeedType.Doobie:
                data.doobieSeeds++;
                break;
            case SeedType.Player:
                data.playerSeeds++;
                break;
        }

        loader.SaveGame();
        return true;
    }
}
