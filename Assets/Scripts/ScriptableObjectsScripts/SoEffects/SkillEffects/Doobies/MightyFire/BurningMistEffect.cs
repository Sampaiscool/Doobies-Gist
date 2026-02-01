using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/MightyFire/BurningMistEffect")]
public class BurningMistEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target, SkillSO skill)
    {
        user.AddEffect(new Effect(EffectType.Hidden, 2, false, 1));

        int spellDamage = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);
        
        target.AddEffect(new Effect(EffectType.Burn, 3, true, spellDamage));
        
        return $"{user.CharacterName} hides in the burning mist and inflicts {spellDamage} burn to {target.CharacterName}";
    }
}
