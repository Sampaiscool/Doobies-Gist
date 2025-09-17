using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/Doobies/Hiroshi/FullBloomSlash")]
public class FullBloomSlashEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddEffect(new Effect(EffectType.BloomBlossom, 3, false, 1));
        return $"{user.CharacterName} Blooms of power!";
    }
}
