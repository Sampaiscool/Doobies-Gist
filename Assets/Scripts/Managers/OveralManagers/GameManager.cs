using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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
    public CombatManager CombatManager;
    public GameObject damageAnimationPrefab;
    public Transform uiCanvas;

    [Header("Doobie Specific")]
    public GameObject goddessButtonsPrefab;

    [Header("Shop Manager")]
    [System.NonSerialized]
    public Upgrade frozenUpgrade = null;

    public bool debugMode = false;
    public bool HasDoneTutorial = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }


    public void ChangeHp(int hpAmount, bool isGain, bool maxHpIncrease)
    {
        PlayerStatsUIManager playerStatsUIManager = FindObjectOfType<PlayerStatsUIManager>();

        if (isGain)
        {
            if (maxHpIncrease)
            {
                CurrentPlayerHP += hpAmount;

                if (playerStatsUIManager != null)
                {
                    playerStatsUIManager.UpdatePlayerInfo();
                }

                return;
            }
            else
            {
                CurrentPlayerHP += hpAmount;
                if (CurrentPlayerHP >= 20)
                {
                    CurrentPlayerHP = 20;
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
        PlayerStatsUIManager playerStatsUIManager = FindObjectOfType<PlayerStatsUIManager>();

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

    public void AfterFight(bool hasWonBattle)
    {
        InCombat = false;

        if (hasWonBattle)
        {
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

                int healAmount = Mathf.Min(10, currentDoobie.MaxHealth - currentDoobie.CurrentHealth);
                currentDoobie.CurrentHealth += healAmount;
            }
        }
        else
        {
            // Penalties for losing
            ChangeSploont(25, true);
            ChangeHp(10, false, false); // Lose 10 Player HP
            currentDoobie.MaxHealth -= 5; // Lose 5 doobie max HP
            currentDoobie.CurrentHealth = currentDoobie.MaxHealth / 2; // Survive with half doobie HP
        }

        PlayerStatsUIManager playerStatsUIManager = FindObjectOfType<PlayerStatsUIManager>();
        if (playerStatsUIManager != null)
        {
            playerStatsUIManager.UpdatePlayerInfo();
        }
    }

    public void SpawnGoddessButtons()
    {
        if (FindObjectOfType<GoddessPanel>() != null)
        {
            Debug.Log("[GameManager] Goddess buttons UI is already active!");
            return;
        }

        Canvas uiCanvas = null;
        foreach (var canvas in FindObjectsOfType<Canvas>())
        {
            if (canvas.isRootCanvas)
            {
                uiCanvas = canvas;
                break;
            }
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
