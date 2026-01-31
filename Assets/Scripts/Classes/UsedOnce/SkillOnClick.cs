using UnityEngine;
using UnityEngine.EventSystems;

public class SkillOnClick : MonoBehaviour, IPointerClickHandler
{
    public SkillSO skill;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (BattleUIManager.Instance == null || skill == null) return;

        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (GameManager.Instance.currentDoobie._so.canMulticast)
            {
                BattleUIManager.Instance.ToggleSkillMultiplierMenu(skill);
            }
        }
    }
}