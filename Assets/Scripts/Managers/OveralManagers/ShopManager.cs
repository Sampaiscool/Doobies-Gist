using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeGroup
{
    public string groupName;
    public CharacterPool characterPool = CharacterPool.None;
    public ResourceType resourceType = ResourceType.None;
    public List<UpgradeSO> upgrades;
}

public class ShopManager : MonoBehaviour
{
    public int refreshCost = 50;

    [SerializeField, Range(0f, 1f)] private float goldenChance = 0.01f;
    private bool isGoldenRound = false;

    [SerializeField] private Transform shopContent;
    [SerializeField] private UpgradeButton upgradeButtonPrefab;

    private bool shopInitialized = false;
    private List<Upgrade> currentUpgrades = new List<Upgrade>();

    [Header("Organized Upgrade Pools")]
    [SerializeField] private List<UpgradeGroup> upgradeGroups;

    public List<Upgrade> GenerateRandomUpgrades(int count, CharacterPool currentPool, ResourceType mainResource)
    {
        isGoldenRound = Random.value < goldenChance;

        var pool = new List<UpgradeSO>();

        if (isGoldenRound)
        {
            // Only from golden group
            foreach (var group in upgradeGroups)
            {
                if (group.characterPool == CharacterPool.Golden)
                    pool.AddRange(group.upgrades);
            }
        }
        else
        {
            foreach (var group in upgradeGroups)
            {
                if (group.characterPool == CharacterPool.None && group.resourceType == ResourceType.None)
                    pool.AddRange(group.upgrades);

                if (group.characterPool == currentPool)
                    pool.AddRange(group.upgrades);

                if (group.resourceType == mainResource)
                    pool.AddRange(group.upgrades);
            }
        }

        List<Upgrade> randomUpgrades = new List<Upgrade>();

        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            UpgradeSO chosen = pool[index];
            pool.RemoveAt(index);

            Upgrade upgradeInstance = new Upgrade(
                chosen.upgradeName,
                chosen.description,
                chosen.cost,
                chosen.type,
                chosen.pool,
                chosen.intensity,
                chosen.isCurse
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

        // Always grab frozen upgrade from GameManager
        Upgrade frozenUpgrade = GameManager.Instance.frozenUpgrade;

        // Ensure frozen upgrade stays in the shop
        if (frozenUpgrade != null && !currentUpgrades.Contains(frozenUpgrade))
        {
            currentUpgrades[0] = frozenUpgrade;
        }

        shopInitialized = true;

        // Clear old buttons
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        // Spawn new buttons
        foreach (var upgrade in currentUpgrades)
        {
            UpgradeButton btn = Instantiate(upgradeButtonPrefab, shopContent);
            btn.Setup(upgrade, HandleBuyUpgrade);

            if (frozenUpgrade == upgrade)
            {
                btn.SetFrozenVisual(true);
                btn.SetLocked(true); // locked so can’t be bought
            }
        }

        Debug.Log("Shop opened with " + currentUpgrades.Count + " upgrades.");
    }

    private void HandleBuyUpgrade(Upgrade upgrade)
    {
        if (GameManager.Instance.frozenUpgrade == upgrade)
        {
            Debug.Log("Cannot buy frozen upgrade: " + upgrade.upgradeName);
            return;
        }

        if (!GameManager.Instance.ChangeSploont(upgrade.cost, false))
        {
            Debug.Log("Not enough Sploont to buy " + upgrade.upgradeName);
            return;
        }

        GameManager.Instance.currentDoobie.AddUpgrade(upgrade);

        foreach (Transform child in shopContent)
        {
            UpgradeButton btn = child.GetComponent<UpgradeButton>();
            if (btn != null && btn.UpgradeData == upgrade)
            {
                Destroy(child.gameObject);
                break;
            }
        }

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

        var currentDoobie = GameManager.Instance.currentDoobie;
        var currentPool = currentDoobie._so.characterPool;
        var mainResource = currentDoobie._so.doobieMainResource;

        List<Upgrade> newUpgrades = GenerateRandomUpgrades(count, currentPool, mainResource);

        // Ensure frozen upgrade is included
        Upgrade frozenUpgrade = GameManager.Instance.frozenUpgrade;
        if (frozenUpgrade != null && !newUpgrades.Contains(frozenUpgrade))
        {
            newUpgrades[0] = frozenUpgrade;
        }

        OpenShop(newUpgrades);
    }

    public void FreezeUpgrade(Upgrade upgrade)
    {
        if (GameManager.Instance.frozenUpgrade == upgrade)
        {
            GameManager.Instance.frozenUpgrade = null; // unfreeze
            Debug.Log("Upgrade unfrozen: " + upgrade.upgradeName);
        }
        else
        {
            GameManager.Instance.frozenUpgrade = upgrade;
            Debug.Log("Upgrade frozen: " + upgrade.upgradeName);
        }

        // Refresh the shop UI so the frozen upgrade is visually locked
        OpenShop(currentUpgrades);
    }

    public void ResetShop()
    {
        shopInitialized = false;
        currentUpgrades.Clear();

        if (GameManager.Instance.frozenUpgrade != null)
        {
            currentUpgrades.Add(GameManager.Instance.frozenUpgrade);
        }

        foreach (Transform child in shopContent)
            Destroy(child.gameObject);
    }

    public bool IsShopInitialized => shopInitialized;
}
