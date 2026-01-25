using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image icon;

    private SkillSO skillData;

    public void Setup(SkillSO skill, System.Action<SkillSO> onClick)
    {
        skillData = skill;

        if (label != null) label.text = skill.skillName;
        if (icon != null) icon.sprite = skill.icon;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke(skillData);

            if (BattleUIManager.Instance != null)
            {
                BattleUIManager.Instance.SkillDescriptionPanel.SetActive(false);
            }
        });

        // Hook up hover
        var hover = GetComponent<SkillUIButtonHover>();
        if (hover != null) hover.skill = skillData;
    }


}
