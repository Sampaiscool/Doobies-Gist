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

    [SerializeField] private Transform doobieAnchor;
    [SerializeField] private Transform vangurrAnchor;

    void Start()
    {
        playerDoobie = GameManager.Instance.currentDoobie;
        enemyVangurr = GameManager.Instance.currentVangurr;

        playerDoobie.animationAnchor = doobieAnchor;
        enemyVangurr.animationAnchor = vangurrAnchor;

        if (playerDoobie is DoobieInstance doobie)
        {
            doobie.currentZurp = doobie._so.zurp;
        }

        AttackButton.onClick.AddListener(OnAttackButtonClicked);
        SkillButton.onClick.AddListener(OnSkillButtonClicked);

        // Open log at start, using CharacterName from CombatantInstance
        BattleUIManager.AddLog($"You face {enemyVangurr.CharacterName}!");
        waitingForNext = true;
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
        string result = chosenSkill.UseSkill(playerDoobie, enemyVangurr);
        Debug.Log(result);

        BattleUIManager.AddLog(result);
        BattleUIManager.UpdateUI();

        IsPlayerTurn = false;
        waitingForNext = true;
    }

    public void OnNextButtonClicked()
    {
        if (!waitingForNext) return;

        if (playerDoobie.CurrentHealth <= 0)
        {
            BattleUIManager.AddLog("You have fallen. The forest grows darker...");
            return;
        }

        if (enemyVangurr.CurrentHealth <= 0)
        {
            BattleUIManager.AddLog($"You have defeated {enemyVangurr.CharacterName}!");
            return;
        }

        if (!IsPlayerTurn)
        {
            // Vangurr's turn
            string result = enemyVangurr.PerformBasicAttack(playerDoobie);
            BattleUIManager.AddLog(result);
            BattleUIManager.UpdateUI();

            IsPlayerTurn = true;
        }
        else
        {
            // Back to player
            BattleUIManager.NextClicked();
            waitingForNext = false;
        }
    }
}
