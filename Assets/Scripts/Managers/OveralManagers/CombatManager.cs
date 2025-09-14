using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public Button AttackButton;
    public Button SkillButton;
    //public Button NextButton;
    public BattleUIManager BattleUIManager;

    public bool IsPlayerTurn = true;
    public bool waitingForNext = false;

    private CombatantInstance playerDoobie;
    private CombatantInstance enemyVangurr;

    private DoobieInstance playerDoobieInstance;
    private VangurrInstance enemyVangurrInstance;

    [SerializeField] private Transform doobieAnchor;
    [SerializeField] private Transform vangurrAnchor;

    void Start()
    {
        playerDoobie = GameManager.Instance.currentDoobie;
        enemyVangurr = GameManager.Instance.currentVangurr;

        playerDoobieInstance = playerDoobie as DoobieInstance;
        enemyVangurrInstance = enemyVangurr as VangurrInstance;

        playerDoobie.animationAnchor = doobieAnchor;
        enemyVangurr.animationAnchor = vangurrAnchor;

        if (playerDoobie is DoobieInstance doobie)
        {
            doobie.ChangeZurp(2, true); // Regain some zurp at start of combat
        }

        AttackButton.onClick.AddListener(OnAttackButtonClicked);
        SkillButton.onClick.AddListener(OnSkillButtonClicked);

        // Open log at start, using CharacterName from CombatantInstance
        BattleUIManager.AddLog($"You face {enemyVangurr.CharacterName}!");
        waitingForNext = true;

        BattleUIManager.UpdateUI();
    }

    void OnAttackButtonClicked()
    {
        if (!IsPlayerTurn || waitingForNext) return;

        string combatResult = playerDoobie.PerformBasicAttack(enemyVangurr);
        BattleUIManager.AddLog(combatResult);
        BattleUIManager.UpdateUI();

        IsPlayerTurn = false;
        waitingForNext = true;
    }

    void OnSkillButtonClicked()
    {
        if (!IsPlayerTurn || waitingForNext) return;

        List<SkillSO> doobieSkills = playerDoobie.GetAllSkills();
        BattleUIManager.DisplaySkills(doobieSkills, OnSkillChosen);
    }

    void OnSkillChosen(SkillSO chosenSkill)
    {
        switch (chosenSkill.resourceUsed)
        {
            case ResourceType.Mana:
                if (GameManager.Instance.currentDoobie.CurrentZurp >= chosenSkill.resourceCost)
                {
                    string result = chosenSkill.UseSkill(playerDoobie, enemyVangurr);
                    Debug.Log(result);

                    BattleUIManager.AddLog(result);
                    BattleUIManager.UpdateUI();

                    IsPlayerTurn = false;
                    waitingForNext = true;
                }
                else
                {
                    BattleUIManager.AddLog("You dont have enough zurp!");
                    BattleUIManager.UpdateUI();
                }
                break;
            case ResourceType.Health:
                if (GameManager.Instance.currentDoobie.CurrentHealth > chosenSkill.resourceCost)
                {
                    string result = chosenSkill.UseSkill(playerDoobie, enemyVangurr);
                    Debug.Log(result);

                    BattleUIManager.AddLog(result);
                    BattleUIManager.UpdateUI();

                    IsPlayerTurn = false;
                    waitingForNext = true;
                }
                else
                {
                    BattleUIManager.AddLog("You dont have enough zurp!");
                    BattleUIManager.UpdateUI();
                }
                break;
            default:
                break;
        }
    }

    public void OnNextButtonClicked()
    {
        if (!waitingForNext) return;

        if (playerDoobie.CurrentHealth <= 0)
        {
            BattleUIManager.AddLog("You have fallen. The forest grows darker...");
            return;
        }
        else if (enemyVangurr.CurrentHealth <= 0)
        {
            BattleUIManager.AddLog($"You have defeated {enemyVangurr.CharacterName}!");

            StartCoroutine(ReturnToAdventureAfterDelay(2f));
            return;
        }
        else
        {
            if (!IsPlayerTurn)
            {
                // Enemy's turn: eerst debuffs aftikken
                enemyVangurr.TickBuffs();
                BattleUIManager.UpdateUI();

                string result = enemyVangurrInstance.PerformTurn(playerDoobie);

                BattleUIManager.AddLog(result);
                BattleUIManager.UpdateUI();

                IsPlayerTurn = true;
            }
            else
            {
                var StunDebuffs = GameManager.Instance.currentDoobie.ActiveBuffs.FindAll(b => b.type == BuffType.Stun);
                if (StunDebuffs.Count > 0)
                {
                    BattleUIManager.AddLog("You are stunned and miss your turn!");

                    playerDoobie.TickBuffs();
                    BattleUIManager.UpdateUI();

                    IsPlayerTurn = false;
                    return;
                }
                else
                {
                    // Player's turn: eerst debuffs aftikken
                    playerDoobie.TickBuffs();
                    BattleUIManager.UpdateUI();

                    BattleUIManager.NextClicked();
                    waitingForNext = false;
                }
            }
        }
    }
    private IEnumerator ReturnToAdventureAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("AdventureScene");
    }
}
