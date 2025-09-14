using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private Transform shopContent;
    [SerializeField] private UpgradeButton upgradeButtonPrefab;

    private bool shopInitialized = false;
    private List<Upgrade> currentUpgrades = new List<Upgrade>();

    [SerializeField] private List<UpgradeSO> allPossibleUpgrades;

    public List<Upgrade> GenerateRandomUpgrades(int count, CharacterPool currentPool)
    {
        List<UpgradeSO> pool = allPossibleUpgrades.FindAll(u =>
            u.pool == currentPool || u.pool == CharacterPool.None);

        List<Upgrade> randomUpgrades = new List<Upgrade>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            UpgradeSO chosen = pool[index];
            pool.RemoveAt(index);

            // Convert SO into runtime instance
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
        if (GameManager.Instance.CurrentPlayerSploont < upgrade.cost)
        {
            Debug.Log("Not enough Sploont to buy " + upgrade.upgradeName);
            return;
        }

        GameManager.Instance.CurrentPlayerSploont -= upgrade.cost;

        GameManager.Instance.currentDoobie.AddUpgrade(upgrade);


        Debug.Log($"Bought {upgrade.upgradeName} for {upgrade.cost} gold!");
    }

    public List<Upgrade> GetCurrentUpgrades()
    {
        return currentUpgrades;
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
