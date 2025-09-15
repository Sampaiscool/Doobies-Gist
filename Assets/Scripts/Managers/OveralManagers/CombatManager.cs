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

    private Dictionary<CombatantInstance, int> combatantTurnCounters = new Dictionary<CombatantInstance, int>();

    void Start()
    {
        playerDoobie = GameManager.Instance.currentDoobie;
        enemyVangurr = GameManager.Instance.currentVangurr;

        playerDoobieInstance = playerDoobie as DoobieInstance;
        enemyVangurrInstance = enemyVangurr as VangurrInstance;

        playerDoobie.animationAnchor = doobieAnchor;
        enemyVangurr.animationAnchor = vangurrAnchor;

        combatantTurnCounters[playerDoobie] = 0;
        combatantTurnCounters[enemyVangurr] = 0;

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

            StartCoroutine(ReturnToAdventureAfterDelay(2f, false));
            return;
        }
        else if (enemyVangurr.CurrentHealth <= 0)
        {
            BattleUIManager.AddLog($"You have defeated {enemyVangurr.CharacterName}!");

            StartCoroutine(ReturnToAdventureAfterDelay(2f, true));
            return;
        }
        else
        {
            if (!IsPlayerTurn)
            {
                CheckTurnBasedUpgrades(enemyVangurr);

                if (CheckForTurnDebuffs(enemyVangurr, out string log))
                {
                    BattleUIManager.AddLog(log);
                    BattleUIManager.UpdateUI();
                    IsPlayerTurn = true;
                    return;
                }

                string result = enemyVangurrInstance.PerformTurn(playerDoobie);
                BattleUIManager.AddLog(result);
                BattleUIManager.UpdateUI();
                IsPlayerTurn = true;
            }
            else
            {
                CheckTurnBasedUpgrades(playerDoobie);

                if (CheckForTurnDebuffs(playerDoobie, out string log))
                {
                    BattleUIManager.AddLog(log);
                    BattleUIManager.UpdateUI();
                    IsPlayerTurn = false;
                    return;
                }

                BattleUIManager.NextClicked();
                waitingForNext = false;
            }
        }
    }
    private bool CheckForTurnDebuffs(CombatantInstance combatant, out string logMessage)
    {
        logMessage = "";

        // Check for Stun first
        var stunDebuffs = combatant.ActiveBuffs.FindAll(b => b.type == BuffType.Stun);
        if (stunDebuffs.Count > 0)
        {
            logMessage = $"{combatant.CharacterName} is stunned and misses their turn!";
            combatant.TickBuffs();
            return true; // turn is skipped
        }

        // Apply burn damage if present
        var burnDebuffs = combatant.ActiveBuffs.FindAll(b => b.type == BuffType.Burn);
        foreach (var burn in burnDebuffs)
        {
            int burnDamage = burn.intensity;
            var(resutl, actualDamage)  = combatant.TakeDamage(burnDamage);
            logMessage += $"\n{combatant.CharacterName} takes {actualDamage} burn damage!";
        }

        // Tick all buffs after applying effects
        combatant.TickBuffs();

        return false; // turn proceeds normally
    }
    private void CheckTurnBasedUpgrades(CombatantInstance combatant)
    {
        if (!combatantTurnCounters.ContainsKey(combatant))
            combatantTurnCounters[combatant] = 0;

        // Increment turn counter
        combatantTurnCounters[combatant]++;

        foreach (var upgrade in combatant.ActiveUpgrades)
        {
            switch (upgrade.type)
            {
                case UpgradeNames.ArcaneMind:
                    if (combatantTurnCounters[combatant] % 3 == 0)
                    {
                        combatant.AddBuff(new Buff(BuffType.SpellStrenghten, 999, false, upgrade.intensity));
                        BattleUIManager.AddLog($"{combatant.CharacterName}'s Arcane Mind empowers them! Spell Strengthen applied.");
                    }
                    break;
            }
        }
    }
    private IEnumerator ReturnToAdventureAfterDelay(float delay, bool playerWon)
    {
        yield return new WaitForSeconds(delay);

        UnityEngine.SceneManagement.SceneManager.LoadScene("AdventureScene");

        GameManager.Instance.AfterFight(playerWon);
    }
}
