using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PanelManager : MonoBehaviour
{
    public GameObject TutorialPanel;
    public GameObject LocationPanel;
    public GameObject VangurrPanel;
    public GameObject ShopPanel;

    public LocationManager locationManager;
    public VangurrManager vangurrManager;
    public PlayerStatsUIManager playerStatsUIManager;

    void Start()
    {
        if (GameManager.Instance.HasDoneTutorial)
        {
            ShowLocationPanel();
        }
        else
        {
            ShowPanel(TutorialPanel);
            GameManager.Instance.HasDoneTutorial = true;
        }
    }

    public void ShowPanel(GameObject panelToShow)
    {
        // Deactivate all known panels
        TutorialPanel.SetActive(false);
        LocationPanel.SetActive(false);
        VangurrPanel.SetActive(false);
        ShopPanel.SetActive(false);

        playerStatsUIManager.UpdatePlayerInfo();

        // Activate the chosen one
        panelToShow.SetActive(true);
    }

    public void ShowLocationPanel()
    {
        ShowPanel(LocationPanel);

        // Generate random locations and display them in the panel
        locationManager.GenerateRandomLocations(3);  // 3 random locations

        // Reset the shop for a new round
        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
            shopManager.ResetShop();
    }

    public void ShowVangurrPanel()
    {
        ShowPanel(VangurrPanel);

        // Only choose a new vangurr if we don't already have one
        if (vangurrManager.ChosenVangurr == null)
        {
            VangurrSO selectedVangurr = vangurrManager.ChooseVangurr();
            GameManager.Instance.currentVangurr = new VangurrInstance(selectedVangurr);

            vangurrManager.UpdateVangurrText(selectedVangurr);
        }
        else
        {
            // Reuse the chosen one
            if (GameManager.Instance.currentVangurr == null)
            {
                GameManager.Instance.currentVangurr = new VangurrInstance(vangurrManager.ChosenVangurr);
            }

            vangurrManager.UpdateVangurrText(vangurrManager.ChosenVangurr);
        }
    }
    public void ShowShopPanel()
    {
        ShowPanel(ShopPanel);

        ShopManager shopManager = FindObjectOfType<ShopManager>();
        if (shopManager != null)
        {
            if (!shopManager.IsShopInitialized)
            {
                var currentPool = GameManager.Instance.currentDoobie._so.characterPool;
                List<Upgrade> upgradesForSale = shopManager.GenerateRandomUpgrades(3, currentPool);
                shopManager.OpenShop(upgradesForSale);
            }
            else
            {
                // If shop is already initialized, just reuse the same upgrades
                shopManager.OpenShop(shopManager.GetCurrentUpgrades());
            }
        }
    }

    public void OnBattlePressed()
    {
        SceneManager.LoadScene("BattleScene");
    }
}
