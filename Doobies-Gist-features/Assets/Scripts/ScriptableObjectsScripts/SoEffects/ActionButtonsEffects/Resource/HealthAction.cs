using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/ResourceActions/HealthAction")]
public class HealthAction : ScriptableObject, IResourceAction
{
    public string ActionName => "Heal";
    public string Description => "Heal a random amount modified by your heal power";
    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        if (user.CurrentHealth != user.MaxHealth)
        {
            float multiplier = UnityEngine.Random.Range(0.5f, 1.5f);
            int healed = Mathf.RoundToInt(user.CurrentHealPower * multiplier);
            int actualHealed = user.HealCombatant(healed);

            return true;
        }
        else
        {
            BattleUIManager.Instance.AddLog($"You are already at Max HP!");
            return false;
        }
    }
}
