using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HarvestShopManager : MonoBehaviour
{
    public TMP_Text harvestText;
    public TMP_Text turipSeedsText;
    public TMP_Text doobieSeedsText;
    public TMP_Text playerSeedsText;

    private TeamLoader loader;

    void Start()
    {
        loader = FindFirstObjectByType<TeamLoader>();
        UpdateUI();
    }

    void UpdateUI()
    {
        if (loader == null || loader.data == null) return;
        var data = loader.data;

        if (harvestText != null)
            harvestText.text = $"Harvest: {data.harvest}";

        if (turipSeedsText != null)
            turipSeedsText.text = $"Turip Seeds: {data.turipSeeds}";

        if (doobieSeedsText != null)
            doobieSeedsText.text = $"Doobie Seeds: {data.doobieSeeds}";

        if (playerSeedsText != null)
            playerSeedsText.text = $"Player Seeds: {data.playerSeeds}";
    }

    public void BuyTuripSeed()
    {
        if (ForestManager.Instance != null && ForestManager.Instance.BuySeed(SeedType.Turip))
        {
            UpdateUI();
        }
    }

    public void BuyDoobieSeed()
    {
        if (ForestManager.Instance != null && ForestManager.Instance.BuySeed(SeedType.Doobie))
        {
            UpdateUI();
        }
    }

    public void BuyPlayerSeed()
    {
        if (ForestManager.Instance != null && ForestManager.Instance.BuySeed(SeedType.Player))
        {
            UpdateUI();
        }
    }

    public void DoneShopping()
    {
        SceneManager.LoadScene("MenuScene");
    }
}
