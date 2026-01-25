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

    public GameObject GoddessSwapButton;
    public GameObject GoddessSwapButtonHolder;

    public LocationManager locationManager;
    public VangurrManager vangurrManager;
    public PlayerStatsUIManager playerStatsUIManager;

    private GameObject currentPanel;

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
        if (currentPanel != null && currentPanel != panelToShow)
        {
            var animOut = currentPanel.GetComponent<PanelAnimator>();
            if (animOut != null)
                animOut.FadeOut();
            else
                currentPanel.SetActive(false);
        }

        playerStatsUIManager.UpdatePlayerInfo();

        var animIn = panelToShow.GetComponent<PanelAnimator>();
        if (animIn != null)
            animIn.FadeIn();
        else
            panelToShow.SetActive(true);

        currentPanel = panelToShow;
    }

    public void ShowLocationPanel()
    {
        ShowPanel(LocationPanel);

        locationManager.GenerateRandomLocations(3);

        ShopManager shopManager = FindFirstObjectByType<ShopManager>();
        if (shopManager != null)
            shopManager.ResetShop();
    }

    public void ShowVangurrPanel()
    {
        ShowPanel(VangurrPanel);

        if (vangurrManager.ChosenVangurr == null)
        {
            VangurrSO selectedVangurr = vangurrManager.ChooseVangurr();
            if (selectedVangurr != null)
            {
                GameManager.Instance.currentVangurr = new VangurrInstance(selectedVangurr);
                GameManager.Instance.currentVangurr.Init();
                vangurrManager.UpdateVangurrText(selectedVangurr);
            }
        }
        else
        {
            if (GameManager.Instance.currentVangurr == null)
            {
                GameManager.Instance.currentVangurr = new VangurrInstance(vangurrManager.ChosenVangurr);
                GameManager.Instance.currentVangurr.Init();
            }

            vangurrManager.UpdateVangurrText(vangurrManager.ChosenVangurr);
        }

        // Goddess swap button visibility
        if (GameManager.Instance.currentDoobie._so.characterPool == CharacterPool.Zelstine)
            GoddessSwapButton.SetActive(true);
        else
            GoddessSwapButton.SetActive(false);
    }


    public void ShowShopPanel()
    {
        ShowPanel(ShopPanel);

        ShopManager shopManager = FindFirstObjectByType<ShopManager>();

        if (shopManager != null)
        {
            if (!shopManager.IsShopInitialized)
            {
                var currentPool = GameManager.Instance.currentDoobie._so.characterPool;
                var mainResource = GameManager.Instance.currentDoobie._so.doobieMainResource;

                List<Upgrade> upgradesForSale = shopManager.GenerateRandomUpgrades(3, currentPool, mainResource);
                shopManager.OpenShop(upgradesForSale);
            }
            else
            {
                shopManager.OpenShop(shopManager.GetCurrentUpgrades());
            }
        }
    }
    public void ShowGoddessButtons()
    {
        GameManager.Instance.SpawnGoddessButtons();
    }

    public void OnBattlePressed()
    {
        SceneManager.LoadScene("BattleScene");
        GameManager.Instance.InCombat = true;
    }
}
