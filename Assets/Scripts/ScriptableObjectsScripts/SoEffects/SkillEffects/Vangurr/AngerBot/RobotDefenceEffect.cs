using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/Effects/AngerBot/RobotDefenceEffect")]
public class RobotDefenceEffect : SkillEffectSO
{
    public override string ApplyEffect(CombatantInstance user, CombatantInstance target)
    {
        user.AddBuff(new Buff(BuffType.Harden, 3, false, 1));

        user.CurrentHealth = Mathf.Min(user.CurrentHealth + 2, user.MaxHealth);

        return $"{user.CharacterName} hardens its armor, gaining 1 Harden and restoring 2 Health!"; 
    }
}
