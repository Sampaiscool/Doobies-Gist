using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public struct BuffVisual
{
    public BuffType type;
    public Sprite icon;
    public GameObject effectPrefab;
}

public class BattleUIManager : MonoBehaviour
{
    [Header("Doobie UI")]
    public Image DoobieImage;
    public TMP_Text DoobieName;
    public TMP_Text DoobieHP;
    public TMP_Text DoobieVurp;

    [Header("Vangurr UI")]
    public Image VangurrImage;
    public TMP_Text VangurrName;
    public TMP_Text VangurrHP;

    [Header("Panels")]
    public GameObject SkillDescriptionPanel;
    public TMP_Text SkillDescriptionText;

    public GameObject CombatLogPanel;
    public TMP_Text CombatLogText;

    public GameObject BattleOptionsPanel;
    public GameObject SkillOptions;

    [Header("Buff Containers")]
    public Transform DoobieBuffsContainer;
    public Transform VangurrBuffsContainer;

    [Header("Skill UI")]
    public Button BackFromSkillsButton;
    public List<Button> AllSkillButtons;
    public List<TMP_Text> SkillButtonLabels;
    public List<Image> SkillButtonIcons;

    [Header("Combatant Stats Panel")]
    public CombatantStatsPanel statsPanelInstance;
    private bool isPanelActive = false;

    [Header("Buff Visuals")]
    public Sprite defaultSprite;
    public GameObject defaultEffectPrefab;

    [Tooltip("Assign each BuffType its icon and effect here")]
    public List<BuffVisual> buffVisuals = new List<BuffVisual>();

    public GameObject BuffIconPrefab;
    public GameObject BuffDescriptionPrefab;

    public static BattleUIManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // STOP IT MAN!!!
        string STOP = isPanelActive.ToString();

        SetupUI();
    }
    /// <summary>
    /// Set up the UI for the first time
    /// </summary>
    private void SetupUI()
    {
        Debug.Log("Setting up UI...");
        if (GameManager.Instance.currentVangurr == null || GameManager.Instance.currentDoobie == null)
        {
            Debug.LogError("Current Vangurr or Doobie is null!");
            return;
        }

        UpdateCombatantUI(GameManager.Instance.currentDoobie, DoobieImage, DoobieName, DoobieHP, DoobieVurp, "Zurp");
        UpdateCombatantUI(GameManager.Instance.currentVangurr, VangurrImage, VangurrName, VangurrHP, null);


        BattleOptionsPanel.SetActive(false);
        SkillOptions.SetActive(false);
        CombatLogText.text = $"You start the fight against {VangurrName.text}";
        ShowPanel(CombatLogPanel);
    }
    /// <summary>
    /// Updates the Ui of the Doobie and Vangurr
    /// </summary>
    public void UpdateUI()
    {
        UpdateCombatantUI(GameManager.Instance.currentDoobie, DoobieImage, DoobieName, DoobieHP, DoobieVurp, "Zurp");
        UpdateCombatantUI(GameManager.Instance.currentVangurr, VangurrImage, VangurrName, VangurrHP, null);

        UpdateBuffsUI(GameManager.Instance.currentDoobie, DoobieBuffsContainer);
        UpdateBuffsUI(GameManager.Instance.currentVangurr, VangurrBuffsContainer);
    }

    public void ShowSpellUI(System.Action<SkillSO> onSkillClicked)
    {
        var doobieSkills = GameManager.Instance.currentDoobie.GetAllSkills();
        DisplaySkills(doobieSkills, onSkillClicked);

        ShowPanel(SkillOptions);
    }

    public void BackFromSpells()
    {
        ShowPanel(BattleOptionsPanel);
    }

    public void NextClicked()
    {
        ShowPanel(BattleOptionsPanel);
    }

    public void AddLog(string result)
    {
        ShowPanel(CombatLogPanel);
        CombatLogText.text = $"{result} ";
    }

    public void DisplaySkills(List<SkillSO> skills, System.Action<SkillSO> onSkillClicked)
    {
        for (int i = 0; i < AllSkillButtons.Count; i++)
        {
            if (i < skills.Count)
            {
                var skill = skills[i];

                AllSkillButtons[i].gameObject.SetActive(true);
                SkillButtonLabels[i].text = skill.skillName;
                if (skill.icon != null)
                    SkillButtonIcons[i].sprite = skill.icon;

                // Make the button *use* the skill
                int capturedIndex = i;
                AllSkillButtons[i].onClick.RemoveAllListeners();
                AllSkillButtons[i].onClick.AddListener(() =>
                {
                    onSkillClicked(skill);
                });

                // Add or update hover behavior
                var hover = AllSkillButtons[i].GetComponent<SkillUIButtonHover>();
                if (hover == null)
                    hover = AllSkillButtons[i].gameObject.AddComponent<SkillUIButtonHover>();

                hover.skill = skill;
            }
            else
            {
                AllSkillButtons[i].gameObject.SetActive(false);
            }
        }

        ShowPanel(SkillOptions);
    }

    private void UpdateCombatantUI(CombatantInstance combatant, Image portraitImage, TMP_Text nameText, TMP_Text hpText, TMP_Text extraText = null, string extraLabel = "")
    {
        if (combatant.so is not ICombatantSO so)
        {
            Debug.LogError("Combatant's SO does not implement ICombatSO!");
            return;
        }

        portraitImage.sprite = so.portrait;
        nameText.text = combatant.CharacterName;
        hpText.text = $"{combatant.CurrentHealth}/{combatant.MaxHealth} HP";

        if (extraText != null)
        {
            if (combatant is DoobieInstance doobie)
            {
                extraText.text = $"{doobie.CurrentZurp}/{doobie.MaxZurp} {extraLabel}";
                extraText.gameObject.SetActive(true);
            }
            else
            {
                extraText.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateBuffsUI(CombatantInstance combatant, Transform buffContainer)
    {
        foreach (var buff in combatant.ActiveBuffs)
        {
            BuffIcon buffIcon = buff.iconInstance;

            if (buffIcon == null)
            {
                // Spawn new icon if it doesn't exist
                GameObject iconGO = Instantiate(BuffIconPrefab, buffContainer, false);
                buffIcon = iconGO.GetComponent<BuffIcon>();
                buff.iconInstance = buffIcon;

                buffIcon.Initialize(buff, GetSpriteForBuffs(buff.type), BuffDescriptionPrefab, GetEffectForBuffs(buff.type));

                // Play effect on first application
                buffIcon.PlayEffect();
            }

            // Optional: update hover
            if (buffIcon.hoverPrefab != null)
            {
                var hover = buffIcon.GetComponent<DebuffIconHover>();
                if (hover != null)
                {
                    hover.linkedBuff = buff;
                    hover.tooltipPrefab = BuffDescriptionPrefab;
                }
            }

            if (!combatant.ActiveBuffIcons.Contains(buffIcon.gameObject))
                combatant.ActiveBuffIcons.Add(buffIcon.gameObject);
        }

        // Remove icons for buffs that no longer exist
        for (int i = combatant.ActiveBuffIcons.Count - 1; i >= 0; i--)
        {
            var iconGO = combatant.ActiveBuffIcons[i];
            if (!combatant.ActiveBuffs.Exists(b => b.iconInstance != null && b.iconInstance.gameObject == iconGO))
            {
                Destroy(iconGO);
                combatant.ActiveBuffIcons.RemoveAt(i);
            }
        }
    }
    public void ShowStats(CombatantInstance combatant)
    {
        if (statsPanelInstance == null) return;

        statsPanelInstance.gameObject.SetActive(true);
        statsPanelInstance.Setup(combatant);
        isPanelActive = true;
    }
    public void HideStats()
    {
        if (statsPanelInstance == null) return;

        statsPanelInstance.gameObject.SetActive(false);
        isPanelActive = false;
    }
    public Sprite GetSpriteForBuffs(BuffType type)
    {
        foreach (var visual in buffVisuals)
            if (visual.type == type && visual.icon != null)
                return visual.icon;
        return defaultSprite;
    }

    public GameObject GetEffectForBuffs(BuffType type)
    {
        foreach (var visual in buffVisuals)
            if (visual.type == type)
            {
                if (visual.effectPrefab != null)
                {
                    Debug.Log($"Returning effectPrefab for {type}: {visual.effectPrefab}");
                    return visual.effectPrefab;
                }
            }

        Debug.Log($"Returning defaultEffectPrefab for {type}: {defaultEffectPrefab}");
        return defaultEffectPrefab;
    }

    private void ShowPanel(GameObject panelToShow)
    {
        CombatLogPanel.SetActive(panelToShow == CombatLogPanel);
        BattleOptionsPanel.SetActive(panelToShow == BattleOptionsPanel);
        SkillOptions.SetActive(panelToShow == SkillOptions);
    }

    public void DisableSkillDescription()
    {
        SkillDescriptionPanel.SetActive(false);
    }
}
