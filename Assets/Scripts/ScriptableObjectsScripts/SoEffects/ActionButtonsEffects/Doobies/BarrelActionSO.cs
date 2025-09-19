using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/DoobieActions/BarrelAction")]
public class BarrelActionSO : ScriptableObject, IDoobieAction
{
    public string ActionName => "Place Barrel";
    public string Description => "Place 5 / 10 barrels";

    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        // Empower your cutlass; 
        int barrelamount = Random.Range(5, 11);
        user.AddEffect(new Effect(EffectType.Barrel, 100, false, barrelamount));

        BattleUIManager.Instance.AddLog($"{user.CharacterName} Places down {barrelamount} barrels!");

        return true;
    }
}
