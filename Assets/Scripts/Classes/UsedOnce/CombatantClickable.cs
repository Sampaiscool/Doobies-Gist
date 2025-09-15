using UnityEngine;

public class CombatantClickable : MonoBehaviour
{
    private CombatantInstance boundInstance;

    public void Bind(CombatantInstance instance)
    {
        boundInstance = instance;
    }

    public void OnClick()
    {
        if (boundInstance != null)
            BattleUIManager.Instance.ShowStats(boundInstance);
    }
}
