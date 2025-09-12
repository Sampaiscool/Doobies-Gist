using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class SkillUIButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public SkillSO skill;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (BattleUIManager.Instance == null || skill == null) return;

        BattleUIManager.Instance.SkillDescriptionPanel.SetActive(true);
        BattleUIManager.Instance.SkillDescriptionText.text =
            $"<b>{skill.skillName}</b>\n" +
            $"Cost: {skill.resourceCost} {(skill.resourceUsed == ResourceType.Mana ? "Zurp" : "HP")}\n\n" +
            $"{skill.description}";
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (BattleUIManager.Instance == null) return;

        BattleUIManager.Instance.SkillDescriptionPanel.SetActive(false);
    }
}
