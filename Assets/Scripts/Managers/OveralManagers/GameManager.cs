using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public DoobieInstance currentDoobie; //The players current Doobie
    public int CurrentDifficulty = 1; //Current Difficulty the player is playing on
    public VangurrInstance currentVangurr; //The Chosen Vangurr the player is going to fight / is fighting.
    public int CurrentPlayerSploont = 0; //The players current Money 
    public CombatManager CombatManager;

    public bool debugMode = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
}
