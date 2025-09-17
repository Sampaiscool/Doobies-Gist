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

        BattleUIManager.UpdateUI();
    }

    void OnAttackButtonClicked()
    {

        string combatResult = playerDoobie.PerformBasicAttack(enemyVangurr);
        BattleUIManager.AddLog(combatResult);
        BattleUIManager.UpdateUI();
    }

    public void OnSkillButtonClicked()
    {
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
    }

    public void OnNextButtonClicked()
    {
        GameObject BattleOptionsPanel = BattleUIManager.BattleOptionsPanel;
        GameObject CombatLogPanel = BattleUIManager.CombatLogPanel;

        // Check defeat conditions first
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

        switch (currentPhase)
        {
            case TurnPhase.Start:
                currentPhase = TurnPhase.PlayerAction;
                BattleUIManager.AddLog($"{playerDoobie.CharacterName}'s turn begins!");

                // UI: show options
                BattleOptionsPanel.SetActive(true);
                CombatLogPanel.SetActive(false);
                break;

            case TurnPhase.PlayerAction:
                // Action was already resolved via Attack/Skill/Heal buttons
                currentPhase = TurnPhase.EnemyAction;
                BattleUIManager.AddLog($"{enemyVangurr.CharacterName}'s turn begins!");

                // UI: hide options, show log
                BattleOptionsPanel.SetActive(false);
                CombatLogPanel.SetActive(true);
                break;

            case TurnPhase.EnemyAction:
                CheckTurnBasedUpgrades(enemyVangurr);

                if (CheckForTurnEffects(enemyVangurr, out string enemyLog))
                {
                    BattleUIManager.AddLog(enemyLog);
                }
                else
                {
                    string result = enemyVangurrInstance.PerformTurn(playerDoobie);
                    BattleUIManager.AddLog(result);
                }

                currentPhase = TurnPhase.EndOfTurn;
                BattleUIManager.AddLog("Press Next to resolve end of turn effects.");

                // UI: still log only
                BattleOptionsPanel.SetActive(false);
                CombatLogPanel.SetActive(true);
                break;

            case TurnPhase.EndOfTurn:
                TickEndOfRound();
                BattleUIManager.AddLog("End of turn effects resolve...");

                // Back to player
                currentPhase = TurnPhase.PlayerAction;
                BattleUIManager.AddLog($"{playerDoobie.CharacterName}'s turn begins!");

                // UI: back to options
                BattleOptionsPanel.SetActive(true);
                CombatLogPanel.SetActive(false);
                break;
        }

        BattleUIManager.UpdateUI();
    }




    private bool CheckForTurnEffects(CombatantInstance combatant, out string logMessage)
    {
        logMessage = "";

        // --- Regeneration ---
        foreach (var regeneration in combatant.ActiveEffects.FindAll(b => b.type == EffectType.Regeneration))
        {
            combatant.HealCombatant(regeneration.intensity);
        }

        // --- Hidden ---
        if (combatant.ActiveEffects.Exists(b => b.type == EffectType.Hidden))
        {
            logMessage = $"{combatant.CharacterName} is hidden and cannot take actions!";
            combatant.TickEffects(); // single tick
            return true; // turn skipped
        }

        // --- Burn ---
        foreach (var burn in combatant.ActiveEffects.FindAll(b => b.type == EffectType.Burn))
        {
            int burnDamage = burn.intensity;
            var (result, actualDamage) = combatant.TakeDamage(burnDamage);
            BattleUIManager.Instance.AddLog($"{combatant.CharacterName} takes {actualDamage} burn damage!");

            if (combatant.CurrentHealth <= 0)
            {
                logMessage = $"{combatant.CharacterName} has fallen!";
                combatant.TickEffects(); // single tick
                return true;
            }
        }

        // --- Stun ---
        if (combatant.ActiveEffects.Exists(b => b.type == EffectType.Stun))
        {
            logMessage = $"{combatant.CharacterName} is stunned and misses their turn!";
            combatant.TickEffects(); // single tick
            return true;
        }

        // --- Always tick once at the end ---
        combatant.TickEffects();

        return false; // proceed normally
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
                        combatant.AddEffect(new Effect(EffectType.SpellStrenghten, 999, false, upgrade.intensity));
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
    private void TickEndOfRound()
    {
        // Tick both combatants’ effects once per full round
        playerDoobie.TickEffects();
        enemyVangurr.TickEffects();

        BattleUIManager.UpdateUI();
        BattleUIManager.AddLog("End of turn effects resolve...");
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
                        combatant.AddEffect(new Effect(EffectType.Deflecion, 999, false, upgrade.intensity));
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

    private TurnPhase currentPhase = TurnPhase.Start;
    private enum TurnPhase
    {
        Start,
        PlayerAction,
        EnemyAction,
        EndOfTurn
    }
}
