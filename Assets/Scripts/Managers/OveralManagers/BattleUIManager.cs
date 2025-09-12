using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleUIManager : MonoBehaviour
{
    public Image DoobieImage;
    public TMP_Text DoobieName;
    public TMP_Text DoobieHP;
    public TMP_Text DoobieVurp;

    public Image VangurrImage;
    public TMP_Text VangurrName;
    public TMP_Text VangurrHP;

    public GameObject SkillDescriptionPanel;
    public TMP_Text SkillDescriptionText;

    public GameObject CombatLogPanel;
    public TMP_Text CombatLogText;
    public GameObject BattleOptionsPanel;
    public GameObject SkillOptions;

    public Button BackFromSkillsButton;
    public List<Button> AllSkillButtons;
    public List<TMP_Text> SkillButtonLabels;

    [Header("buff Icons")]
    public Sprite defaultSprite;
    public Sprite defenceDownSprite;
    

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
        hpText.text = $"{combatant.CurrentHealth}/{so.baseHealth} HP";

        if (extraText != null)
        {
            if (combatant is DoobieInstance doobie && doobie._so is DoobieSO doobieSO)
            {
                extraText.text = $"{doobie.currentZurp} {extraLabel}";
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
        // Eerst oude iconen opruimen
        foreach (var icon in combatant.ActiveBuffIcons)
            Destroy(icon);

        combatant.ActiveBuffIcons.Clear();

        // Voor elke actieve buff een icoon maken
        foreach (var buff in combatant.ActiveBuffs)
        {
            GameObject iconGO = Instantiate(BuffIconPrefab, buffContainer, false);

            Image iconImage = iconGO.GetComponent<Image>();
            iconImage.sprite = GetSpriteForBuffs(buff.type);

            // Hover tooltip instellen
            var hover = iconGO.GetComponent<DebuffIconHover>();
            if (hover != null)
            {
                hover.linkedBuff = buff;
                hover.tooltipPrefab = BuffDescriptionPrefab;
            }

            combatant.ActiveBuffIcons.Add(iconGO);
        }
    }
    private Sprite GetSpriteForBuffs(BuffType type)
    {
        return type switch
        {
            BuffType.DefenceDown => defenceDownSprite,
            _ => defaultSprite
        };
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
