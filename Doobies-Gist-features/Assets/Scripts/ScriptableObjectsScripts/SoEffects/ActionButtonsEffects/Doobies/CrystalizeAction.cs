using UnityEngine;

[CreateAssetMenu(menuName = "SO/DoobieActions/CrystalizeAction")]
public class CrystalizeAction : ScriptableObject, IDoobieAction
{
    public string ActionName => "Crystalize";
    public string Description => "Gain 1 Stun and then gain 1 Crystalize";

    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.Stun, 2, true, 1));
        user.AddEffect(new Effect(EffectType.Crystalize, 6, false, 1));

        BattleUIManager.Instance.AddLog($"{user.CharacterName} crystallizes themselves, Stunning them and granting them 1 Crystalize!");

        return true;
    }
}
