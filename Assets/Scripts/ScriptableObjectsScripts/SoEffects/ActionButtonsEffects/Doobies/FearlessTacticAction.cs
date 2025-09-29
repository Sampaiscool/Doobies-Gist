using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/DoobieActions/FearlessTacticAction")]
public class FearlessTacticAction : ScriptableObject, IDoobieAction
{
    public string ActionName => "Fearless Tactic";
    public string Description => "Blind yourself your 2 turns; Gain weapon/spell strenghten";

    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Blind, 2, true, 1));
        user.AddEffect(new Effect(EffectType.WeaponStrenghten, 2, true, 2));
        user.AddEffect(new Effect(EffectType.SpellStrenghten, 2, true, 2));

        BattleUIManager.Instance.AddLog($"{user.CharacterName} blinds themself; giving them weapon/spell strenghten");

        return true;
    }
}
