using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Vangurr/Difficulty3/MagicSpright/VeilBurstEffect")]
public class VeilBurstEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        int baseDmg = user.GetEffectiveSkillDamage(user.CurrentSkillDmg);
        var (result, dmgDone) = target.TakeDamage(baseDmg);

        string log = $"{user.CharacterName} bursts into shadow, damaging {target.CharacterName} for {dmgDone} damage and vanishing briefly.";

        user.AddEffect(new Effect(EffectType.Hidden, 1, false, 1));

        Effect shadowEffect = user.ActiveEffects.Find(s => s.type == EffectType.Shadow);
        if (shadowEffect != null)
        {
            int shadowIntensity = shadowEffect.intensity;

            target.AddEffect(new Effect(EffectType.Stun, shadowIntensity, true, shadowIntensity));

            BattleUIManager.Instance.AddLog($"{user.CharacterName}'s veil twists reality, stunning {target.CharacterName}!");
            log += $" The veil’s darkness echoed {shadowIntensity} times!";
        }

        return log;
    }
}
