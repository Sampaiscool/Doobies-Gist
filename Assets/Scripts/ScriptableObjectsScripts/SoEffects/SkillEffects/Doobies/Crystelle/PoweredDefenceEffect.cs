using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Crystelle/PoweredDefence")]
public class PoweredDefenceEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        Effect crystalize = user.ActiveEffects.Find(c => c.type == EffectType.Crystalize);
        if (crystalize != null)
        {
            user.AddEffect(new Effect(EffectType.Harden, 3, false, crystalize.intensity));
            BattleUIManager.Instance.AddLog($"{user.CharacterName} uses their crystalline power to harden");
        }

        int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        int roundedDefence = (int)user.CurrentDefence;

        int Armourded = baseDmg * roundedDefence;

        var (result, damageDone) = target.TakeDamage(Armourded, true);

        return $"{user.CharacterName} channels their crystalline defence to deal {damageDone} damage to {target.CharacterName}!";
    }
}
