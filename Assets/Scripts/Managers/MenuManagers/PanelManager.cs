using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [Header("Skip Buttons")]
    public GameObject DefaultSkipButton;
    public GameObject MightyFireSkipButton;
    
    [Header("Shop Buttons")]
    public GameObject BurnUpgradeButton;

    public LocationManager locationManager;
    public VangurrManager vangurrManager;
    public PlayerStatsUIManager playerStatsUIManager;

    [Header("Lists")]
    public List<GameObject> AllSkipButtons;
    public List<GameObject> AllShopButtons;
    
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
        
        GetCorrectSkipButton(GameManager.Instance.currentDoobie._so.characterPool);

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
        
        GetCorrectShopButton(GameManager.Instance.currentDoobie._so.characterPool);
        
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

    public void GetCorrectSkipButton(CharacterPool currentPool)
    {
        foreach (var skipButton in AllSkipButtons)
        {
            skipButton.SetActive(false);
        }
        
        switch (currentPool)
        {
            case CharacterPool.MightyFire:
                MightyFireSkipButton.SetActive(true);
                break;
            default:
                DefaultSkipButton.SetActive(true);
                break;
        }
    }

    public void GetCorrectShopButton(CharacterPool currentPool)
    {
        foreach (var shopButton in AllShopButtons)
        {
            shopButton.SetActive(false);
        }
    
        BurnUpgradeButton.SetActive(false);

        // 2. Check de pool
        switch (currentPool)
        {
            case CharacterPool.MightyFire:
                int burnLevel = GameManager.Instance.currentDoobie.CurrentBurnLevel;

                if (burnLevel < 3)
                {
                    BurnUpgradeButton.SetActive(true);
                    TMP_Text buttonText = BurnUpgradeButton.GetComponentInChildren<TMP_Text>();
                
                    int cost = (burnLevel == 1) ? 1000 : 2000;
                    buttonText.text = $"Upgrade burn for {cost} sploont";
                } 
                break;
            default:
                break;
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
