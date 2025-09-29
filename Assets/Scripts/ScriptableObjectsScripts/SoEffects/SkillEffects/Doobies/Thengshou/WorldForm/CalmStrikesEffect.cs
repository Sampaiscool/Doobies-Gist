using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Thengshou/WorldForm/CalmStrikesEffect")]
public class CalmStrikesEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveWeaponDamageAfterEffects(user.GetEffectiveWeaponDamage());

        int halvedDmg = baseDmg / 2;

        var (result, damageDone) = target.TakeDamage(halvedDmg);

        user.AddEffect(new Effect(EffectType.Regeneration, 3, false, damageDone));

        if (user is DoobieInstance doobie && doobie.MainResource is SoulflowResource soulflow)
        {
            soulflow.SpiritEnergy.Gain(2);
        }

        return $"{user.CharacterName} strikes {target.CharacterName} calmly, dealing {damageDone} damage and gaining {damageDone} regeneration!";
    }
}
