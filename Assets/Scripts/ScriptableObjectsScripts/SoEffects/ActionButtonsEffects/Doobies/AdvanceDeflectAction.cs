using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/DoobieActions/AdvanceDeflectAction")]
public class AdvanceDeflectAction : ScriptableObject, IDoobieAction
{
    public string ActionName => "Advance Deflect";

    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        var deflectEffect = user.ActiveEffects.Find(d => d.type == EffectType.Deflecion);

        if (deflectEffect == null)
        {
            BattleUIManager.Instance.AddLog($"{user.CharacterName} tries to deflect, but has no energy stored!");
            return false; // FAIL, don’t end turn
        }

        BattleUIManager.Instance.AddLog($"{user.CharacterName} consumes all his deflects to unleash a powerful attack!");

        int weaponDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());
        weaponDmg += 2;

        target.TakeDamage(deflectEffect.intensity + weaponDmg);
        user.ActiveEffects.Remove(deflectEffect);

        return true; // SUCCESS, consume turn
    }
}

