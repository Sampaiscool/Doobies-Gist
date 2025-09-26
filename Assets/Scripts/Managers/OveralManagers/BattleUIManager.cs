using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct EffectVisual
{
    public string label;
    public EffectType type;
    public Sprite icon;
    public GameObject effectPrefab;
}
[System.Serializable]
public struct TransformationVisual
{
    public string label;
    public Transformations type;
    public Sprite image;
}

public class BattleUIManager : MonoBehaviour
{
    [Header("GeneralUI")]
    public TMP_Text nextButtontext;

    [Header("Doobie UI")]
    public Image DoobieImage;
    public TMP_Text DoobieName;
    public TMP_Text DoobieHP;
    public TMP_Text DoobieVurp;

    [Header("Skill UI")]
    [SerializeField] private Transform skillButtonContainer; // Where buttons go
    [SerializeField] private SkillButton skillButtonPrefab;  // Your prefab

    [Header("Vangurr UI")]
    public Image VangurrImage;
    public TMP_Text VangurrName;
    public TMP_Text VangurrHP;

    [Header("Floating HP Text")]
    public GameObject floatingTextPrefab;   // assign prefab in inspector
    public Transform worldCanvas;           // the canvas to spawn under

    [Header("Panels")]
    public GameObject SkillDescriptionPanel;
    public TMP_Text SkillDescriptionText;

    [Header("Combat Log")]
    public GameObject CombatLogPanel;      // ScrollView panel
    public Transform CombatLogContent;     // Content inside ScrollView
    public GameObject CombatLogEntryPrefab;// Prefab for each log line
    public Button ExpandLogButton;         // Optional: expands the panel

    public GameObject BattleOptionsPanel;
    public GameObject SkillOptions;

    [Header("Effect Containers")]
    public Transform DoobieEffectsContainer;
    public Transform VangurrEffectsContainer;

    [Header("Skill UI")]
    public Button BackFromSkillsButton;
    public List<Button> AllSkillButtons;
    public List<TMP_Text> SkillButtonLabels;
    public List<Image> SkillButtonIcons;

    [Header("Combatant Stats Panel")]
    public CombatantStatsPanel statsPanelInstance;
    private bool isPanelActive = false;

    [Header("Effect Visuals")]
    public Sprite defaultSprite;
    public GameObject defaultEffectPrefab;

    [Tooltip("Assign each EffectType its icon and effect here")]
    public List<EffectVisual> EffectVisuals = new List<EffectVisual>();

    [Header("Transformation")]
    [Tooltip("Assign all the diffrent transformation sprites here")]
    public List<TransformationVisual> TransformationVisuals = new List<TransformationVisual>();

    public GameObject EffectIconPrefab;
    public GameObject EffectDescriptionPrefab;

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

        // Update portraits and health/resource
        UpdateCombatantUI(GameManager.Instance.currentDoobie, DoobieImage, DoobieName, DoobieHP, DoobieVurp, "Zurp");
        UpdateCombatantUI(GameManager.Instance.currentVangurr, VangurrImage, VangurrName, VangurrHP, null);

        BattleOptionsPanel.SetActive(false);
        SkillOptions.SetActive(false);

        AddLog($"You start the fight against {VangurrName.text}");
        ShowPanel(CombatLogPanel);

        //GameManager.Instance.currentDoobie.SetGoddess(GoddessType.Kaelyth, false);

        // Now refresh skills
        var skills = GameManager.Instance.currentDoobie.GetAllSkills();
        RefreshSkillButtons(skills);
    }
    /// <summary>
    /// Updates the Ui of the Doobie and Vangurr
    /// </summary>
    public void UpdateUI()
    {
        UpdateCombatantUI(GameManager.Instance.currentDoobie, DoobieImage, DoobieName, DoobieHP, DoobieVurp, "Zurp");
        UpdateCombatantUI(GameManager.Instance.currentVangurr, VangurrImage, VangurrName, VangurrHP, null);

        UpdateEffectsUI(GameManager.Instance.currentDoobie, DoobieEffectsContainer);
        UpdateEffectsUI(GameManager.Instance.currentVangurr, VangurrEffectsContainer);
    }

    public void ShowSpellUI()
    {
        var skills = GameManager.Instance.currentDoobie.GetAllSkills();
        RefreshSkillButtons(skills);
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

    public void AddLog(string message)
    {
        ShowPanel(CombatLogPanel);

        float time = Time.time;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        string timestamp = $"[{minutes:00}:{seconds:00}]";

        GameObject entryGO = Instantiate(CombatLogEntryPrefab, CombatLogContent);
        TMP_Text entryText = entryGO.GetComponentInChildren<TMP_Text>();
        if (entryText != null)
            entryText.text = $"{timestamp} {message}";

        // Wait until end of frame so the layout updates
        StartCoroutine(SlideInAfterLayout(entryGO, 0.3f));

        // Flash background on child panel
        Image bg = entryGO.GetComponentInChildren<Image>();
        if (bg != null)
            StartCoroutine(FlashBackground(bg, Color.yellow, 0.2f));

        ScrollToBottom();
    }

    public void ScrollToBottom()
    {
        StartCoroutine(ScrollToBottomNextFrame());
    }
    private IEnumerator ScrollToBottomNextFrame()
    {
        // wait until the end of the frame so Unity finishes laying out
        yield return new WaitForEndOfFrame();

        Canvas.ForceUpdateCanvases();
        ScrollRect scroll = CombatLogContent.GetComponentInParent<ScrollRect>();
        if (scroll != null)
            scroll.verticalNormalizedPosition = 1f; // works with bottom-left pivot
    }
    public void ToggleCombatLogSize()
    {
        RectTransform rt = CombatLogPanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = rt.sizeDelta.y > 200 ? new Vector2(rt.sizeDelta.x, 200) : new Vector2(rt.sizeDelta.x, 400);
        }
    }

    public void DisplaySkills(List<SkillSO> skills, System.Action<SkillSO> onSkillChosen)
    {
        ShowPanel(SkillOptions);

        foreach (Transform child in skillButtonContainer)
            Destroy(child.gameObject);

        foreach (var skill in skills)
        {
            var button = Instantiate(skillButtonPrefab, skillButtonContainer);
            button.Setup(skill, onSkillChosen);
        }
    }
    /// <summary>
    /// Standard skill selection callback
    /// </summary>
    public void HandleSkillSelected(SkillSO skill)
    {
        if (GameManager.Instance.currentDoobie != null && GameManager.Instance.currentVangurr != null)
        {
            string result = skill.UseSkill(GameManager.Instance.currentDoobie, GameManager.Instance.currentVangurr);
            AddLog(result);
            BackFromSpells();
            UpdateUI();
        }
    }

    public void RefreshSkillButtons(List<SkillSO> skills)
    {
        DisplaySkills(skills, HandleSkillSelected);
    }

    private void UpdateCombatantUI(CombatantInstance combatant, Image portraitImage, TMP_Text nameText, TMP_Text hpText, TMP_Text extraText = null, string extraLabel = "")
    {
        if (combatant.so is not ICombatantSO so)
        {
            Debug.LogError("Combatant's SO does not implement ICombatSO!");
            return;
        }

        portraitImage.sprite = combatant.CurrentImage;
        nameText.text = combatant.CharacterName;
        hpText.text = $"{combatant.CurrentHealth}/{combatant.MaxHealth} HP";

        if (extraText != null)
        {
            if (combatant is DoobieInstance doobie2 &&
                doobie2.MainResource != null &&
                doobie2.MainResource is SoulflowResource soulflow)
            {
                extraText.text = $"{soulflow.WorldEnergy.Current}/{soulflow.WorldEnergy.Max} World\n " +
                    $"{soulflow.SpiritEnergy.Current}/{soulflow.SpiritEnergy.Max} Spirit";
                extraText.gameObject.SetActive(true);
            }
            else if (combatant is DoobieInstance doobie &&
                doobie.MainResource != null &&
                doobie.MainResource.Type != ResourceType.Health)
            {
                extraText.text = $"{doobie.MainResource.Current}/{doobie.MainResource.Max} {doobie.MainResource.Type}";
                extraText.gameObject.SetActive(true);
            }
            
            else
            {
                extraText.gameObject.SetActive(false);
            }
        }

    }

    public void UpdateEffectsUI(CombatantInstance combatant, Transform effectContainer)
    {
        foreach (var effect in combatant.ActiveEffects)
        {
            EffectIcon effectIcon = effect.iconInstance;

            if (effectIcon == null)
            {
                GameObject iconGO = Instantiate(EffectIconPrefab, effectContainer, false);
                effectIcon = iconGO.GetComponent<EffectIcon>();
                effect.iconInstance = effectIcon;

                effectIcon.Initialize(effect, GetSpriteForEffects(effect.type), EffectDescriptionPrefab, GetAnimationForEffects(effect.type));

                effectIcon.PlayEffect();
            }

            if (effectIcon.hoverPrefab != null)
            {
                var hover = effectIcon.GetComponent<EffectIconHover>();
                if (hover != null)
                {
                    hover.linkedEffect = effect;
                    hover.tooltipPrefab = EffectDescriptionPrefab;
                }
            }

            if (!combatant.ActiveEffectIcons.Contains(effectIcon.gameObject))
                combatant.ActiveEffectIcons.Add(effectIcon.gameObject);
        }

        for (int i = combatant.ActiveEffectIcons.Count - 1; i >= 0; i--)
        {
            var iconGO = combatant.ActiveEffectIcons[i];
            if (!combatant.ActiveEffects.Exists(b => b.iconInstance != null && b.iconInstance.gameObject == iconGO))
            {
                Destroy(iconGO);
                combatant.ActiveEffectIcons.RemoveAt(i);
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
    public Sprite GetSpriteForEffects(EffectType type)
    {
        foreach (var visual in EffectVisuals)
            if (visual.type == type && visual.icon != null)
                return visual.icon;
        return defaultSprite;
    }

    public GameObject GetAnimationForEffects(EffectType type)
    {
        foreach (var visual in EffectVisuals)
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
    public void SpawnFloatingText(string message, Color color, Transform anchor, bool isDamage)
    {
        if (floatingTextPrefab == null || anchor == null) return;

        GameObject obj = Instantiate(floatingTextPrefab, anchor, false);
        FloatingHPText ft = obj.GetComponent<FloatingHPText>();

        obj.transform.localPosition = Vector3.zero;
        obj.transform.localScale = Vector3.one;

        // Base direction
        Vector3 baseDir = isDamage ? Vector3.down : Vector3.up;

        // Add random offset in X/Y
        float angle = Random.Range(-30f, 30f);
        Quaternion rotation = Quaternion.Euler(0, 0, angle);
        Vector3 dir = rotation * baseDir;

        ft.Setup(message, color, dir.normalized);
    }
    public void CombatantTransformation(CombatantInstance combatant, Transformations transformation)
    {
        if (transformation == Transformations.None)
        {
            if (combatant is DoobieInstance doobie)
            {
                combatant.CurrentImage = doobie._so.portrait;
            }
            else if (combatant is VangurrInstance vangurr) 
            {
                combatant.CurrentImage = vangurr._so.portrait;
            }
        }
        combatant.CurrentImage = GetTransformationImage(transformation);
        UpdateUI();
    }
    public Sprite GetTransformationImage(Transformations transformation)
    {
        foreach (var visual in TransformationVisuals)
            if (visual.type == transformation)
            {
                if (visual.image != null)
                {
                    Debug.Log($"Returning image for {transformation}: {visual.image}");
                    return visual.image;
                }
            }
        // Fallback!! should never happen
        Debug.LogWarning("Transformation Image not set!");
        return DoobieImage.sprite;
    }

    private IEnumerator SlideInAfterLayout(GameObject entryGO, float duration)
    {
        // Wait until layout is updated
        yield return new WaitForEndOfFrame();

        RectTransform rect = entryGO.GetComponent<RectTransform>();
        Vector2 target = rect.anchoredPosition;

        // Slide from 50 units above
        Vector2 start = target + new Vector2(0, 50f);
        rect.anchoredPosition = start;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            rect.anchoredPosition = Vector2.Lerp(start, target, t);
            yield return null;
        }

        rect.anchoredPosition = target;
    }

    private IEnumerator FlashBackground(Image bg, Color flashColor, float duration)
    {
        Color original = bg.color;
        bg.color = flashColor;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bg.color = Color.Lerp(flashColor, original, elapsed / duration);
            yield return null;
        }
        bg.color = original;
    }
}
