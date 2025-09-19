using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "SO/DoobieActions/FlowerPowerAction")]
public class FlowerPowerAction : ScriptableObject, IDoobieAction
{
    public string ActionName => "Petal Explosion";
    public string Description => "Deal damage with your heal power then take the same damage";

    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveHealPower(user.CurrentHealPower);

        var (result, damageDone) = target.TakeDamage(baseDmg);

        user.TakeDamage(damageDone);

        BattleUIManager.Instance.AddLog($"{user.CharacterName} uses their healing power to do {damageDone}!\n Then {user.CharacterName} takes {damageDone} damage!");

        return true;
    }
}
