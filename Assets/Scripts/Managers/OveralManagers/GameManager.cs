using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int CurrentDifficulty = 1; //Current Difficulty the player is playing on
    public int BattlesFought = 0; //How many battles the player has fought
    public int MaxBattlesBeforeBoss; //How many battles the player has to fight before a boss battle

    public DoobieInstance currentDoobie; //The players current Doobie

    public VangurrInstance currentVangurr; //The Chosen Vangurr the player is going to fight / is fighting.

    public bool InCombat = false;

    public int CurrentPlayerSploont = 0; //The players current Money
    public int CurrentPlayerHP = 20;
    public int CurrentPlayerMaxHP = 20;
    public int CurrentPlayerDzeef = 0;
    public int CurrentHarvest = 0; //The players current Harvest earned during the run
    public CombatManager CombatManager;
    public PanelManager PanelManager;
    public LocationManager locationManager;
    public InfoManager InfoManager;
    public TeamSelectUI TeamSelectUI;
    public GameObject damageAnimationPrefab;
    public Transform uiCanvas;

    // Multiplier applied to the next chosen location's effect. Default 1 (no extra procs).
    public int nextLocationMultiplier = 1;

    [Header("Settings")]
    public GameObject SettingsPanel;

    [Header("Doobie Specific")]
    public GameObject goddessButtonsPrefab;

    [Header("Shop Manager")]
    [System.NonSerialized]
    public Upgrade frozenUpgrade = null;

    public bool debugMode = false;
    public bool HasDoneTutorial = false;

    public TeamLoader loader;

    private void Awake()
    {
        FindManagers();
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Settings existing = FindFirstObjectByType<Settings>();
            if (existing != null)
            {
                Destroy(existing.gameObject);
            }
            else
            {
                SpawnSettings();
            }
        }
    }

    public void FindManagers()
    {
        CombatManager = FindFirstObjectByType<CombatManager>();
        PanelManager = FindFirstObjectByType<PanelManager>();
        locationManager = FindFirstObjectByType<LocationManager>();
        InfoManager = FindFirstObjectByType<InfoManager>();
        
        loader = FindFirstObjectByType<TeamLoader>();
        TeamSelectUI = FindFirstObjectByType<TeamSelectUI>();
    }

    public void ChangeHp(int hpAmount, bool isGain, bool maxHpIncrease)
    {
        PlayerStatsUIManager playerStatsUIManager = FindFirstObjectByType<PlayerStatsUIManager>();

        if (isGain)
        {
            if (maxHpIncrease)
            {
                CurrentPlayerHP += hpAmount;
                CurrentPlayerMaxHP += hpAmount;

                if (playerStatsUIManager != null)
                {
                    playerStatsUIManager.UpdatePlayerInfo();
                }

                return;
            }
            else
            {
                CurrentPlayerHP += hpAmount;
                if (CurrentPlayerHP >= CurrentPlayerMaxHP)
                {
                    CurrentPlayerHP = CurrentPlayerMaxHP;
                }

                if (playerStatsUIManager != null)
                {
                    playerStatsUIManager.UpdatePlayerInfo();
                }

                return;
            }
        }
        else
        {
            CurrentPlayerHP -= hpAmount;
            if (CurrentPlayerHP <= 0)
            {
                CurrentPlayerHP = 0;
                EndRun();
                Debug.Log("Game Over! Player has run out of HP.");
            }

            if (playerStatsUIManager != null)
            {
                playerStatsUIManager.UpdatePlayerInfo();
            }
        }
    }
    /// <summary>
    /// Changes the players current Sploonnt amount.
    /// </summary>
    /// <param name="sploontAmount">The amout that it gets changed by</param>
    /// <param name="isGain">true = player gains sploont / false = player loses sploont</param>
    /// <returns>Wheter the player has enough sploont to reduce</returns>
    public bool ChangeSploont(int sploontAmount, bool isGain)
    {
        PlayerStatsUIManager playerStatsUIManager = FindFirstObjectByType<PlayerStatsUIManager>();

        if (isGain)
        {
            CurrentPlayerSploont += sploontAmount;

            if (playerStatsUIManager != null)
            {
                playerStatsUIManager.UpdatePlayerInfo();
            }

            return true;
        }
        else
        {
            if (CurrentPlayerSploont - sploontAmount < 0)
            {
                Debug.Log("Not enough Sploont!");
                return false;
            }
            else
            {
                CurrentPlayerSploont -= sploontAmount;

                if (playerStatsUIManager != null)
                {
                    playerStatsUIManager.UpdatePlayerInfo();
                }

                return true;
            }
        }
    }
    public bool ChangeDzeef(int amount, bool isGain)
    {
        PlayerStatsUIManager playerStatsUIManager = FindFirstObjectByType<PlayerStatsUIManager>();

        if (isGain)
        {
            CurrentPlayerDzeef += amount;

            if (playerStatsUIManager != null)
            {
                playerStatsUIManager.UpdatePlayerInfo();
            }

            return true;
        }
        else
        {
            if (playerStatsUIManager != null)
            {
                playerStatsUIManager.UpdatePlayerInfo();
            }

            if (CurrentPlayerDzeef < amount)
                return false;

            CurrentPlayerDzeef -= amount;
            return true;
        }

    }

    public void AfterFight(bool hasWonBattle)
    {
        InCombat = false;
        currentDoobie.ActiveEffects.Clear();

        if (hasWonBattle)
        {
            CurrentHarvest++;
            ChangeSploont(50, true);

            bool isBossFight = BattlesFought >= MaxBattlesBeforeBoss;

            if (!isBossFight)
            {
                // Regular fight: increment counter
                BattlesFought++;
            }
            else
            {
                // Boss fight won: reset counter AND increase difficulty
                BattlesFought = 0;
                CurrentDifficulty++;
                ChangeDzeef(1, true);

                // --- NIEUW: Beloning naar JSON op basis van difficulty ---
                if (loader != null && loader.data != null)
                {
                    // Beloning: 5 harvest per difficulty level
                    int bossBonus = CurrentDifficulty * 5;
                    loader.data.harvest += bossBonus;
                    Debug.Log($"Boss defeated! Gained {bossBonus} persistent harvest.");
                }

                // --- NIEUW: Laat bomen groeien na een Boss ---
                // We checken eerst of de ForestManager bestaat
                if (ForestManager.Instance != null)
                {
                    ForestManager.Instance.GrowBattleTrees(1);
                }

                int healAmount = Mathf.Min(10, currentDoobie.MaxHealth - currentDoobie.CurrentHealth);
                currentDoobie.CurrentHealth += healAmount;
            
                // Sla de progressie direct op
                loader.SaveGame();
            }
        }
        else
        {
            // Penalties for losing
            ChangeSploont(25, true);
            ChangeHp(10, false, false); 
            currentDoobie.MaxHealth -= 5; 
            currentDoobie.CurrentHealth = currentDoobie.MaxHealth / 2; 
        }

        PlayerStatsUIManager playerStatsUIManager = FindFirstObjectByType<PlayerStatsUIManager>();
        if (playerStatsUIManager != null)
        {
            playerStatsUIManager.UpdatePlayerInfo();
        }
    }

    public void EndRun()
    {
        FindManagers();
        string currentDoobie = loader.data.selectedDoobieName;

        loader.AddSavedXP(10);

        // Transfer run Harvest to persistent data for spending on seeds
        loader.data.harvest += CurrentHarvest;
        CurrentHarvest = 0;

        loader.SaveGame();

        // Load Harvest Shop scene instead of immediately going to menu
        SceneManager.LoadScene("HarvestShopScene");
    }

    public void SpawnSettings()
    {
        if (FindFirstObjectByType<Settings>() != null)
        {
            Debug.Log("[GameManager] Settings UI is already active!");
            return;
        }

        Canvas uiCanvas = null;
        Canvas canvasObject = FindFirstObjectByType<Canvas>();
        if (canvasObject.isRootCanvas)
        {
            uiCanvas = canvasObject;
        }

        if (SettingsPanel == null || uiCanvas == null)
        {
            Debug.LogWarning("[GameManager] Missing prefab or no Canvas found in scene!");
            return;
        }

        GameObject obj = Instantiate(SettingsPanel, uiCanvas.transform);

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }

    public void SpawnGoddessButtons()
    {
        if (FindFirstObjectByType<GoddessPanel>() != null)
        {
            Debug.Log("[GameManager] Goddess buttons UI is already active!");
            return;
        }

        Canvas uiCanvas = null;
        Canvas canvasObject = FindFirstObjectByType<Canvas>();
        if (canvasObject.isRootCanvas)
        {
            uiCanvas = canvasObject;
        }

        if (goddessButtonsPrefab == null || uiCanvas == null)
        {
            Debug.LogWarning("[GameManager] Missing prefab or no Canvas found in scene!");
            return;
        }

        GameObject obj = Instantiate(goddessButtonsPrefab, uiCanvas.transform);

        RectTransform rect = obj.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }

}
