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

        var clickInterceptor = GetComponent<SkillOnClick>();
        if (clickInterceptor != null)
            clickInterceptor.skill = skillData;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            onClick?.Invoke(skillData);
            BattleUIManager.Instance?.SkillDescriptionPanel.SetActive(false);
        });

        var hover = GetComponent<SkillUIButtonHover>();
        if (hover != null) hover.skill = skillData;
    }

}

