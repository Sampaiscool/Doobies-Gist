using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeGroup
{
    public string groupName; // optional for editor clarity
    public CharacterPool characterPool; // Which character this group belongs to
    public List<UpgradeSO> upgrades; // All upgrades in this group
}

public class ShopManager : MonoBehaviour
{
    public int refreshCost = 50;

    [SerializeField] private Transform shopContent;
    [SerializeField] private UpgradeButton upgradeButtonPrefab;

    private bool shopInitialized = false;
    private List<Upgrade> currentUpgrades = new List<Upgrade>();

    [Header("Organized Upgrade Pools")]
    [SerializeField] private List<UpgradeGroup> upgradeGroups;

    public List<Upgrade> GenerateRandomUpgrades(int count, CharacterPool currentPool)
    {
        // Find the pool for the current character, plus generic upgrades (None)
        var pool = new List<UpgradeSO>();

        foreach (var group in upgradeGroups)
        {
            if (group.characterPool == currentPool || group.characterPool == CharacterPool.None)
            {
                pool.AddRange(group.upgrades);
            }
        }

        List<Upgrade> randomUpgrades = new List<Upgrade>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            UpgradeSO chosen = pool[index];
            pool.RemoveAt(index);

            // Convert SO into runtime Upgrade instance
            Upgrade upgradeInstance = new Upgrade(
                chosen.upgradeName,
                chosen.description,
                chosen.cost,
                chosen.type,
                chosen.pool,
                chosen.intensity
            )
            {
                icon = chosen.icon
            };

            randomUpgrades.Add(upgradeInstance);
        }

        return randomUpgrades;
    }

    public void OpenShop(List<Upgrade> upgradesForSale)
    {
        currentUpgrades = upgradesForSale;
        shopInitialized = true;

        // Clear old buttons
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        // Spawn new buttons
        foreach (var upgrade in upgradesForSale)
        {
            UpgradeButton btn = Instantiate(upgradeButtonPrefab, shopContent);
            btn.Setup(upgrade, HandleBuyUpgrade);
        }

        Debug.Log("Shop opened with " + upgradesForSale.Count + " upgrades.");
    }
    private void HandleBuyUpgrade(Upgrade upgrade)
    {
        if (!GameManager.Instance.ChangeSploont(upgrade.cost, false))
        {
            Debug.Log("Not enough Sploont to buy " + upgrade.upgradeName);
            return;
        }

        // Give the upgrade to the player (stacking allowed)
        GameManager.Instance.currentDoobie.AddUpgrade(upgrade);

        // Remove only the button visually, but keep the upgrade in the pool
        foreach (Transform child in shopContent)
        {
            UpgradeButton btn = child.GetComponent<UpgradeButton>();
            if (btn != null && btn.UpgradeData == upgrade)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        // Remove from current shop list to prevent duplicate buttons in the same refresh
        currentUpgrades.Remove(upgrade);

        Debug.Log($"Bought {upgrade.upgradeName} for {upgrade.cost} gold!");
    }


    public List<Upgrade> GetCurrentUpgrades()
    {
        return currentUpgrades;
    }
    public void RefreshShop(int count = 3)
    {
        if (!GameManager.Instance.ChangeSploont(refreshCost, false))
        {
            Debug.Log("Not enough Sploont to refresh");
            return;
        }

        if (!shopInitialized) return;

        // Get the current character pool
        var currentPool = GameManager.Instance.currentDoobie._so.characterPool;

        // Generate a fresh set of upgrades
        List<Upgrade> newUpgrades = GenerateRandomUpgrades(count, currentPool);

        // Open shop with the new upgrades
        OpenShop(newUpgrades);
    }

    public void ResetShop()
    {
        shopInitialized = false;
        currentUpgrades.Clear();

        foreach (Transform child in shopContent)
            Destroy(child.gameObject);
    }

    public bool IsShopInitialized => shopInitialized;
}
