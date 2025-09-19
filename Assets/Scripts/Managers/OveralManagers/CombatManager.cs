using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    public Transform ButtonContainer; // where buttons live
    public GameObject ButtonPrefab;

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

    private TurnPhase currentPhase = TurnPhase.Start;
    private enum TurnPhase { Start, PlayerTurnStart, PlayerAction, EnemyTurnStart, EnemyAction, EndOfTurn }

    void Start()
    {
        playerDoobie = GameManager.Instance.currentDoobie;
        enemyVangurr = GameManager.Instance.currentVangurr;

        playerDoobieInstance = playerDoobie as DoobieInstance;
        enemyVangurrInstance = enemyVangurr as VangurrInstance;

        playerDoobie.animationAnchor = doobieAnchor;
        enemyVangurr.animationAnchor = vangurrAnchor;

        doobieImageBinder?.Bind(playerDoobie);
        vangurrImageBinder?.Bind(enemyVangurr);

        combatantTurnCounters[playerDoobie] = 0;
        combatantTurnCounters[enemyVangurr] = 0;

        // Give small starting resource if Doobie uses Zurp (only when they have that as main resource)
        if (playerDoobie is DoobieInstance doobie && doobie.MainResource != null && doobie.MainResource.Type == ResourceType.Zurp)
        {
            doobie.MainResource.Gain(2);
        }

        OnBattleStartEffects();

        SpawnBattleOptions();

        // initial log
        BattleUIManager.AddLog($"You face {enemyVangurr.CharacterName}!");

        // start in Start phase; user must press Next to begin player turn
        currentPhase = TurnPhase.Start;
        waitingForNext = true;
        IsPlayerTurn = false;

        BattleUIManager.UpdateUI();
    }
    
    public void SpawnBattleOptions()
    {
        if (ButtonContainer == null || ButtonPrefab == null)
        {
            Debug.LogError("[CombatManager] Can't spawn buttons. Container or Prefab missing.");
            return;
        }

        // Clear old buttons
        foreach (Transform t in ButtonContainer)
            Destroy(t.gameObject);

        var player = GameManager.Instance.currentDoobie;

        // Always: Attack + Skill
        CreateButton("Attack", OnAttackButtonClicked, "Use your basic attack.");
        CreateButton("Skill", OnSkillButtonClicked, "Use your skills.");

        // Resource button
        if (player.so is DoobieSO doobieSO && doobieSO.resourceActionSO is IResourceAction resource)
        {
            CreateButton(resource.ActionName, () =>
            {
                bool success = resource.Execute(player, enemyVangurr);
                if (success) OnActionButtonClicked();
                else BattleUIManager.UpdateUI();
            }, resource.Description);
        }

        // Doobie-specific action
        if (player.so is DoobieSO doobieSO2 && doobieSO2.doobieActionSO is IDoobieAction action)
        {
            CreateButton(action.ActionName, () =>
            {
                bool success = action.Execute(player, enemyVangurr);
                if (success) OnActionButtonClicked();
                else BattleUIManager.UpdateUI();
            }, action.Description);
        }

        // Force UI rebuild so layout shows right away
        var rect = ButtonContainer as RectTransform;
        if (rect != null)
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rect);

        Debug.Log($"[CombatManager] Spawned {ButtonContainer.childCount} buttons.");
    }

    public void CreateButton(string label, UnityEngine.Events.UnityAction onClick, string tooltipText = "")
    {
        if (ButtonPrefab == null || ButtonContainer == null) return;

        GameObject obj = Instantiate(ButtonPrefab, ButtonContainer, false);
        obj.SetActive(true);

        // Set label
        var tmp = obj.GetComponentInChildren<TextMeshProUGUI>();
        if (tmp != null) tmp.text = label;

        // Setup button callback
        var btn = obj.GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(onClick);
        }
        else
        {
            Debug.LogWarning("[CombatManager] Instantiated button prefab has no Button component.");
        }

        // Setup tooltip via the BattleOptionButton script
        var battleBtn = obj.GetComponent<BattleOptionButton>();
        if (battleBtn != null)
        {
            battleBtn.Setup(label, onClick, tooltipText);
        }
        else if (!string.IsNullOrEmpty(tooltipText))
        {
            Debug.LogWarning("[CombatManager] Button prefab missing BattleOptionButton script for tooltip.");
        }

        Debug.Log($"[CombatManager] Created button '{label}'");
    }
    private void OnActionButtonClicked()
    {
        BattleUIManager.UpdateUI();

        waitingForNext = true;
        currentPhase = TurnPhase.EnemyTurnStart;
        IsPlayerTurn = false;
    }

    // ---------- Player input handlers ----------
    private void OnAttackButtonClicked()
    {
        if (currentPhase != TurnPhase.PlayerAction || waitingForNext) return;

        string combatResult = playerDoobie.PerformBasicAttack(enemyVangurr);
        BattleUIManager.AddLog(combatResult);
        BattleUIManager.UpdateUI();

        waitingForNext = true;
        currentPhase = TurnPhase.EnemyTurnStart;
        IsPlayerTurn = false;
    }

    private void OnSkillButtonClicked()
    {
        if (currentPhase != TurnPhase.PlayerAction || waitingForNext) return;

        List<SkillSO> doobieSkills = playerDoobie.GetAllSkills();
        BattleUIManager.Instance.DisplaySkills(doobieSkills, OnSkillChosen);
    }

    void OnSkillChosen(SkillSO chosenSkill)
    {
        // ensure still player action
        if (currentPhase != TurnPhase.PlayerAction || waitingForNext) return;

        var doobie = GameManager.Instance.currentDoobie;

        bool canPay = false;

        switch (chosenSkill.resourceUsed)
        {
            // Universal: Health
            case ResourceType.Health:
                canPay = doobie.CurrentHealth > chosenSkill.resourceCost;
                break;

            default: // main resource (zurp or other)
                if (doobie.MainResource != null && doobie.MainResource.Type == chosenSkill.resourceUsed)
                    canPay = doobie.MainResource.Current >= chosenSkill.resourceCost;
                break;
        }

        if (!canPay)
        {
            BattleUIManager.AddLog($"You don’t have enough {chosenSkill.resourceUsed}!");
            BattleUIManager.UpdateUI();
            // remain in player action, don't advance
            waitingForNext = false;
            return;
        }

        // pay and execute
        string result = chosenSkill.UseSkill(playerDoobie, enemyVangurr);
        Debug.Log(result);
        BattleUIManager.AddLog(result);
        BattleUIManager.UpdateUI();

        // finalize player action -> require Next to proceed
        waitingForNext = true;
        currentPhase = TurnPhase.EnemyTurnStart;
        IsPlayerTurn = false;
    }

    // ---------- Next button / phase handler ----------
    public void OnNextButtonClicked()
    {
        // if we are locked (waiting) but next is still allowed by state — allow it.
        // Ensure we don't proceed while the UI expects an earlier action
        if (!waitingForNext && currentPhase != TurnPhase.Start && currentPhase != TurnPhase.PlayerTurnStart) return;

        // safety check for defeat before doing anything
        if (CheckAndHandleDefeat()) return;

        // short refs to UI panels (expect these exist)
        GameObject BattleOptionsPanel = BattleUIManager.BattleOptionsPanel;
        GameObject CombatLogPanel = BattleUIManager.CombatLogPanel;

        switch (currentPhase)
        {
            case TurnPhase.Start:
                // Begin the very first player's turn
                BeginPlayerTurn();
                break;

            case TurnPhase.PlayerTurnStart:
                // Shouldn't usually get here, but treat as "show options" step
                BeginPlayerTurn();
                break;

            case TurnPhase.PlayerAction:
                // Player pressed Next without acting -> treat as skip
                BattleUIManager.AddLog($"{playerDoobie.CharacterName} skips their action.");
                BattleUIManager.UpdateUI();

                // lock until player takes their action (but we move to enemy now)
                waitingForNext = true;
                currentPhase = TurnPhase.EnemyTurnStart;
                IsPlayerTurn = false;
                break;

            case TurnPhase.EnemyTurnStart:
                // Enemy pre-turn checks and action happen here
                BeginEnemyTurn();
                break;

            case TurnPhase.EnemyAction:
                // Not used if we do enemy action immediately in BeginEnemyTurn
                BeginEnemyTurn();
                break;

            case TurnPhase.EndOfTurn:
                // resolve end-of-round effects (tick durations)
                TickEndOfRound();

                // after ticking, check for deaths
                if (CheckAndHandleDefeat()) return;

                // go to the next player's turn start — require Next to actually show/enter it
                currentPhase = TurnPhase.PlayerTurnStart;
                waitingForNext = true; // user presses Next to start player's turn
                BattleUIManager.AddLog("Press Next to start the next round.");
                BattleUIManager.UpdateUI();
                break;
        }

        BattleUIManager.UpdateUI();
        UpdateNextButtonLabel();
    }
    private void UpdateNextButtonLabel()
    {
        TMP_Text nextButtonText = BattleUIManager.Instance.nextButtontext;
        nextButtonText.text = $"Next ({currentPhase})";
    }

    // ---------- Helper flow methods ----------
    private void BeginPlayerTurn()
    {
        // Start-of-turn upgrades / counters (increments counter once)
        CheckTurnBasedUpgrades(playerDoobie);

        // Apply per-turn effects (regen/burn/hidden/stun). This does not tick durations.
        bool skipOrDeath = CheckForTurnEffects(playerDoobie, out string log);
        if (!string.IsNullOrEmpty(log)) BattleUIManager.AddLog(log);
        BattleUIManager.UpdateUI();

        if (CheckAndHandleDefeat()) return;

        if (skipOrDeath)
        {
            // skip player's action, proceed to enemy start
            currentPhase = TurnPhase.EnemyTurnStart;
            waitingForNext = true;
            IsPlayerTurn = false;

            BattleUIManager.AddLog($"{playerDoobie.CharacterName}'s action is skipped this turn.");
            return;
        }

        // show player options
        BattleUIManager.AddLog($"{playerDoobie.CharacterName}'s turn begins!");
        BattleUIManager.BattleOptionsPanel.SetActive(true);
        BattleUIManager.CombatLogPanel.SetActive(false);

        currentPhase = TurnPhase.PlayerAction;
        waitingForNext = false; // allow player to click attack/skill/heal
        IsPlayerTurn = true;
    }

    private void BeginEnemyTurn()
    {
        // increments and upgrades for enemy
        CheckTurnBasedUpgrades(enemyVangurr);

        bool skipOrDeath = CheckForTurnEffects(enemyVangurr, out string log);
        if (!string.IsNullOrEmpty(log)) BattleUIManager.AddLog(log);
        BattleUIManager.UpdateUI();

        if (CheckAndHandleDefeat()) return;

        if (skipOrDeath)
        {
            // skip enemy action -> go straight to EndOfTurn
            currentPhase = TurnPhase.EndOfTurn;
            waitingForNext = true;
            BattleUIManager.AddLog($"{enemyVangurr.CharacterName}'s action is skipped this turn.");
            return;
        }

        // perform enemy action immediately
        string result = enemyVangurrInstance.PerformTurn(playerDoobie);
        BattleUIManager.AddLog(result);
        BattleUIManager.UpdateUI();

        if (CheckAndHandleDefeat()) return;

        // next step: end of turn
        currentPhase = TurnPhase.EndOfTurn;
        waitingForNext = true;
        IsPlayerTurn = false;

        BattleUIManager.AddLog("Press Next to resolve end of turn effects.");
    }

    private bool CheckForTurnEffects(CombatantInstance combatant, out string logMessage)
    {
        logMessage = "";

        // --- Hidden (skip turn) ---
        if (combatant.ActiveEffects.Exists(e => e.type == EffectType.Hidden))
        {
            logMessage += $"{combatant.CharacterName} is hidden and cannot take actions!\n";
            return true; // skip this combatant's action (do not tick durations here)
        }

        // --- Stun (skip) ---
        if (combatant.ActiveEffects.Exists(e => e.type == EffectType.Stun))
        {
            logMessage += $"{combatant.CharacterName} is stunned and misses their turn!\n";
            return true;
        }

        return false; // no skip/death -> proceed normally
    }

    private void CheckTurnBasedUpgrades(CombatantInstance combatant)
    {
        if (!combatantTurnCounters.ContainsKey(combatant))
            combatantTurnCounters[combatant] = 0;

        // Increment turn counter (happens once per turn start)
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

                    int eatDamage = combatantTurnCounters[combatant];
                    target.TakeDamage(eatDamage);

                    BattleUIManager.AddLog($"{combatant.CharacterName} feasts on {target.CharacterName}, dealing {eatDamage} damage!");
                    break;

                case UpgradeNames.PhanthomTouch:
                    if (combatantTurnCounters[combatant] % 5 == 0)
                    {
                        if (combatant is VangurrInstance)
                        {
                            playerDoobieInstance.KillInstance();
                        }
                        else if (combatant is DoobieInstance)
                        {
                            enemyVangurrInstance.KillInstance();
                        }
                    }
                    break;
                case UpgradeNames.Careless:
                    if (combatantTurnCounters[combatant] % 1 == 0)
                    {
                        combatant.AddEffect(new Effect(EffectType.DefenceDown, 2, true, upgrade.intensity));
                        combatant.AddEffect(new Effect(EffectType.WeaponStrenghten, 2, false, upgrade.intensity));
                        combatant.AddEffect(new Effect(EffectType.SpellStrenghten, 2, false, upgrade.intensity));

                        BattleUIManager.Instance.AddLog($"{combatant.CharacterName} Tries a careless tactic!");
                    }
                    break;
                        default:
                    break;
            }
        }
    }

    private void TickEndOfRound()
    {
        ApplyEndOfTurnEffects(playerDoobie);
        ApplyEndOfTurnEffects(enemyVangurr);

        BattleUIManager.UpdateUI();
        BattleUIManager.AddLog("End of turn effects resolve...");
    }

    private void ApplyEndOfTurnEffects(CombatantInstance combatant)
    {
        string name = combatant.CharacterName;

        // --- Regeneration ---
        foreach (var regen in combatant.ActiveEffects.FindAll(e => e.type == EffectType.Regeneration))
        {
            int healed = combatant.HealCombatant(regen.intensity);
            if (healed > 0)
            {
                BattleUIManager.Instance.AddLog($"{name} regenerates {healed} HP.");

                Upgrade feelingGreenUpgrade = combatant.ActiveUpgrades.Find(f => f.type == UpgradeNames.FeelingGreen);
                if (feelingGreenUpgrade != null)
                {
                    combatant.HealCombatant(feelingGreenUpgrade.intensity);
                }
            }
        }

        // --- Burn ---
        foreach (var burn in combatant.ActiveEffects.FindAll(e => e.type == EffectType.Burn))
        {
            var (result, damageDone) = combatant.TakeDamage(burn.intensity);
            BattleUIManager.Instance.AddLog($"{name} takes {damageDone} burn damage!");
        }

        // --- Fleeting Life ---
        foreach (var upgrade in combatant.ActiveUpgrades.FindAll(e => e.type == UpgradeNames.FleetingLife))
        {
            var (result, damageDone) = combatant.TakeDamage(upgrade.intensity);
            BattleUIManager.Instance.AddLog($"{combatant.CharacterName} life fleets away, taking {damageDone} damage.");
        }

        // Finally tick durations down
        combatant.TickEffects();
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

    private bool CheckAndHandleDefeat()
    {
        if (playerDoobie.CurrentHealth <= 0)
        {
            BattleUIManager.AddLog("You have fallen. The forest grows darker...");
            StartCoroutine(ReturnToAdventureAfterDelay(2f, false));
            return true;
        }

        if (enemyVangurr.CurrentHealth <= 0)
        {
            BattleUIManager.AddLog($"You have defeated {enemyVangurr.CharacterName}!");
            StartCoroutine(ReturnToAdventureAfterDelay(2f, true));
            return true;
        }

        return false;
    }

    private IEnumerator ReturnToAdventureAfterDelay(float delay, bool playerWon)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("AdventureScene");
        GameManager.Instance.AfterFight(playerWon);
    }
}
