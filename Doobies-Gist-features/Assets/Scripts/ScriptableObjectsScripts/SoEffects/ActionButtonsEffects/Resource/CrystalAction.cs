using UnityEngine;


[CreateAssetMenu(menuName = "SO/ResourceActions/CrystalAction")]
public class CrystalAction : ScriptableObject, IResourceAction
{
    public string ActionName => "Bauxite Defence";
    public string Description => "Gain 1 Harden for each Crystalized intensity";
    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        Effect crystalize = user.ActiveEffects.Find(c => c.type == EffectType.Crystalize);
        
        if (crystalize != null)
        {
            user.AddEffect(new Effect(EffectType.Harden, 3, false, crystalize.intensity));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} gains {crystalize.intensity} Harden from their Crystalize effect!");
            return true;
        }
        else
        {
            BattleUIManager.Instance.AddLog($"{user.CharacterName} has no Crystalize effect to gain Harden from!");
            return false;
        }
    }
}
