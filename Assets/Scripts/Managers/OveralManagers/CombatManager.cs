using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public Button AttackButton;
    public Button SkillButton;
    public Button HealButton;

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

    [SerializeField] private CombatantClickable doobieImageBinder;
    [SerializeField] private CombatantClickable vangurrImageBinder;

    private Dictionary<CombatantInstance, int> combatantTurnCounters = new Dictionary<CombatantInstance, int>();

    void Start()
    {
        playerDoobie = GameManager.Instance.currentDoobie;
        enemyVangurr = GameManager.Instance.currentVangurr;

        playerDoobieInstance = playerDoobie as DoobieInstance;
        enemyVangurrInstance = enemyVangurr as VangurrInstance;

        playerDoobie.animationAnchor = doobieAnchor;
        enemyVangurr.animationAnchor = vangurrAnchor;

        doobieImageBinder.Bind(playerDoobie);
        vangurrImageBinder.Bind(enemyVangurr);

        combatantTurnCounters[playerDoobie] = 0;
        combatantTurnCounters[enemyVangurr] = 0;

        if (playerDoobie is DoobieInstance doobie && doobie.MainResource != null && doobie.MainResource.Type == ResourceType.Zurp)
        {
            doobie.MainResource.Gain(2);
        }

        OnBattleStartEffects();

        AttackButton.onClick.AddListener(OnAttackButtonClicked);
        SkillButton.onClick.AddListener(OnSkillButtonClicked);
        HealButton.onClick.AddListener(OnHealButtonClicked);

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

    public void OnSkillButtonClicked()
    {
        if (!IsPlayerTurn || waitingForNext) return;

        List<SkillSO> doobieSkills = playerDoobie.GetAllSkills();
        BattleUIManager.Instance.DisplaySkills(doobieSkills, OnSkillChosen);
    }

    void OnSkillChosen(SkillSO chosenSkill)
    {
        var doobie = GameManager.Instance.currentDoobie;

        bool canPay = false;

        switch (chosenSkill.resourceUsed)
        {
            // Any "Universal" resource
            case ResourceType.Health:
                canPay = doobie.CurrentHealth > chosenSkill.resourceCost;
                break;

            default: // Any "main" resource
                if (doobie.MainResource != null && doobie.MainResource.Type == chosenSkill.resourceUsed)
                    canPay = doobie.MainResource.Current >= chosenSkill.resourceCost;
                break;
        }

        if (canPay)
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
            BattleUIManager.AddLog($"You don’t have enough {chosenSkill.resourceUsed}!");
            BattleUIManager.UpdateUI();
        }
    }

    void OnHealButtonClicked()
    {
        float multiplier = Random.Range(0.5f, 1.5f);

        int healed = Mathf.RoundToInt(playerDoobie.CurrentHealPower * multiplier);

        int actualHealed = playerDoobie.HealCombatant(healed);

        string combatResult = $"{playerDoobie.CharacterName} heals for {actualHealed} health!";
        BattleUIManager.AddLog(combatResult);
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

        var hiddenBuffs = combatant.ActiveBuffs.FindAll(b => b.type == BuffType.Hidden);
        if (hiddenBuffs.Count > 0)
        {
            logMessage = $"{combatant.CharacterName} is hidden and cannot take actions!";
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

            if (combatant.CurrentHealth <= 0)
            {
                logMessage += $"\n{combatant.CharacterName} has fallen!";
                return true;
            }
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

                case UpgradeNames.GremlinHunger:
                    CombatantInstance target = (combatant is DoobieInstance)
                        ? GameManager.Instance.currentVangurr
                        : GameManager.Instance.currentDoobie;

                    if (!combatantTurnCounters.ContainsKey(combatant))
                        combatantTurnCounters[combatant] = 0;

                    int eatDamage = combatantTurnCounters[combatant];
                    target.TakeDamage(eatDamage);

                    BattleUIManager.AddLog($"{combatant.CharacterName} feasts on {target.CharacterName}, dealing {eatDamage} damage!");
                    break;
                case UpgradeNames.PhanthomTouch:
                    if (combatantTurnCounters[combatant] % 5 == 0)
                    {
                        if(combatant is VangurrInstance vangurr)
                        {
                            playerDoobieInstance.KillInstance();
                        }
                        else if(combatant is DoobieInstance doobie)
                        {
                            enemyVangurrInstance.KillInstance();
                        }

                    }
                    break;
                default:
                    break;
            }
        }
    }
    void OnBattleStartEffects()
    {
        List<CombatantInstance> combatants = new List<CombatantInstance> { playerDoobie, enemyVangurr };

        foreach (var combatant in combatants)
        {
            if (combatant.ActiveUpgrades != null)
            {
                foreach (var upgrade in combatant.ActiveUpgrades)
                {
                    if (upgrade.type == UpgradeNames.StayPrepared)
                    {
                        combatant.AddBuff(new Buff(BuffType.Deflecion, 999, false, upgrade.intensity));
                    }

                    if (upgrade.type == UpgradeNames.HealtySupplies)
                    {
                        combatant.HealCombatant(combatant.CurrentHealPower + upgrade.intensity);
                    }

                    if (upgrade.type == UpgradeNames.GremlinHunger)
                    {
                        combatantTurnCounters[combatant] = 0 + upgrade.intensity;
                        BattleUIManager.AddLog($"{combatant.CharacterName} prepares to feast this battle...");
                    }
                }
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
