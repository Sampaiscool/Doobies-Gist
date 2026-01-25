using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/DoobieActions/AdvanceDeflectAction")]
public class AdvanceDeflectAction : ScriptableObject, IDoobieAction
{
    public string ActionName => "Advance Deflect";
    public string Description => "Consume all your Deflection; use your basic attack twice and then attack with increased damage.";

    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        var deflectEffect = user.ActiveEffects.Find(d => d.type == EffectType.Deflecion);

        if (deflectEffect == null)
        {
            BattleUIManager.Instance.AddLog($"{user.CharacterName} tries to deflect, but has no energy stored!");
            return false; // FAIL, don’t end turn
        }

        BattleUIManager.Instance.AddLog($"{user.CharacterName} consumes all his deflects to unleashing 3 powefull attacks!");

        int weaponDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());
        weaponDmg *= 2;

        var (result, damageDone) = target.TakeDamage(deflectEffect.intensity + weaponDmg, true);

        string log1 = user.PerformBasicAttack(GameManager.Instance.currentVangurr);
        string log2 = user.PerformBasicAttack(GameManager.Instance.currentVangurr);

        BattleUIManager.Instance.AddLog($"{log1}");
        BattleUIManager.Instance.AddLog($"{log2}");
        BattleUIManager.Instance.AddLog($"Finaly {user.CharacterName} attacks dealing {damageDone} damage!");

        user.ActiveEffects.Remove(deflectEffect);

        return true;
    }
}

