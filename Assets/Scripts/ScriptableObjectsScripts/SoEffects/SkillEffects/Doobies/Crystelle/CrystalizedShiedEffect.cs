using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Crystelle/CrystalizedShied")]
public class CrystalizedShiedEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.HealCombatant(user.MaxHealth / 4);
        user.AddEffect(new Effect(EffectType.Shield, 3, false, user.MaxHealth / 10));

        Effect harden = user.ActiveEffects.Find(h => h.type == EffectType.Harden);
        Effect crystalize = user.ActiveEffects.Find(h => h.type == EffectType.Crystalize);

        if (crystalize != null && harden != null && harden.intensity >= 5)
        {
            user.AddEffect(new Effect(EffectType.HardHitter, 5, false, crystalize.intensity));

            BattleUIManager.Instance.AddLog($"{user.CharacterName}'s crystalized shield empowers them, granting Hard Hitter!");
        }

        return $"{user.CharacterName} crystallizes a shield around them, healing them and gaining a shield that absorbs {user.MaxHealth / 10} damage!";
    }
}
