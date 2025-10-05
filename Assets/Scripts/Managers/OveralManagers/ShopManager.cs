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

[System.Serializable]
public class ItemGroup
{
    public string groupName;
    public CharacterPool characterPool = CharacterPool.None;
    public ResourceType resourceType = ResourceType.None;
    public List<ItemSO> items;
}


public class ShopManager : MonoBehaviour
{
    public int refreshCost = 50;

    [SerializeField, Range(0f, 1f)] private float goldenChance = 0.01f;
    private bool isGoldenRound = false;
    private bool viewingDzeefShop = false;

    [SerializeField] private Transform shopContent;
    [SerializeField] private UpgradeButton upgradeButtonPrefab;

    private bool shopInitialized = false;
    private List<Upgrade> currentUpgrades = new List<Upgrade>();

    [Header("Organized Upgrade Pools")]
    [SerializeField] private List<UpgradeGroup> upgradeGroups;

    [Header("Organized Item Pools")]
    [SerializeField] private List<ItemGroup> itemGroups;

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
    public void ToggleShopMode()
    {
        viewingDzeefShop = !viewingDzeefShop;

        if (viewingDzeefShop)
        {
            OpenDzeefShop();
        }
        else
        {
            OpenShop(currentUpgrades);
        }
    }
    public void OpenDzeefShop()
    {
        foreach (Transform child in shopContent)
            Destroy(child.gameObject);

        var currentPool = GameManager.Instance.currentDoobie._so.characterPool;
        var mainResource = GameManager.Instance.currentDoobie._so.doobieMainResource;

        var pool = new List<ItemSO>();

        // Collect valid items based on pool rules
        foreach (var group in itemGroups)
        {
            if (group.characterPool == CharacterPool.None && group.resourceType == ResourceType.None)
                pool.AddRange(group.items);

            if (group.characterPool == currentPool)
                pool.AddRange(group.items);

            if (group.resourceType == mainResource)
                pool.AddRange(group.items);
        }

        List<Item> shopItems = new List<Item>();
        int count = 3;
        for (int i = 0; i < count && pool.Count > 0; i++)
        {
            int index = Random.Range(0, pool.Count);
            ItemSO chosen = pool[index];
            pool.RemoveAt(index);

            Item item = new Item(
                chosen.itemName,
                chosen.description,
                chosen.cost,
                chosen.type,
                chosen.pool
            )
            {
                icon = chosen.icon
            };

            shopItems.Add(item);
        }

        // Spawn item buttons
        foreach (var item in shopItems)
        {
            UpgradeButton btn = Instantiate(upgradeButtonPrefab, shopContent);
            btn.SetupAsItem(item, HandleBuyItem);
        }

        Debug.Log($"Opened Dzeef Shop with {shopItems.Count} items from {currentPool} / {mainResource}");
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
    private void HandleBuyItem(Item item)
    {
        if (!GameManager.Instance.ChangeDzeef(item.cost, false))
        {
            Debug.Log("Not enough Dzeef to buy " + item.itemName);
            return;
        }

        GameManager.Instance.currentDoobie.AddItem(item);

        foreach (Transform child in shopContent)
        {
            UpgradeButton btn = child.GetComponent<UpgradeButton>();
            if (btn != null && btn.ItemData == item)
            {
                Destroy(child.gameObject);
                break;
            }
        }

        Debug.Log($"Bought {item.itemName} for {item.cost} Dzeef!");
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
