using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/DoobieActions/RitualOfTheThree")]
public class RitualOfTheThreeAction : ScriptableObject, IDoobieAction
{
    public string ActionName => "Ritual of the Three";
    public string Description => "1 - Gain 2 faith\n2 - Debuff yourself; Gain 4 faith\n3 - Heal yourself, Debuff and smite the enemy!";

    // Tracks stage per user instance
    private Dictionary<CombatantInstance, int> ritualStages = new Dictionary<CombatantInstance, int>();

    public bool Execute(CombatantInstance user, CombatantInstance target)
    {
        if (!ritualStages.ContainsKey(user))
            ritualStages[user] = 0; 

        int stage = ritualStages[user];

        switch (stage)
        {
            case 0:
                BattleUIManager.Instance.AddLog($"{user.CharacterName} whispers a prayer and gains 2 Faith.");

                if (user is DoobieInstance doobie)
                {
                    doobie.MainResource.Gain(2);
                }

                break;

            case 1:
                BattleUIManager.Instance.AddLog($"{user.CharacterName} sacrifices her body to the gods, gaining 4 Faith but weakening herself.");

                if (user is DoobieInstance doobie2)
                {
                    doobie2.MainResource.Gain(4);
                    user.AddEffect(new Effect(EffectType.WeaponWeaken, 2, true, 1));
                    user.AddEffect(new Effect(EffectType.SpellWeaken, 2, true, 1));
                }

                break;

            case 2:
                BattleUIManager.Instance.AddLog($"{user.CharacterName} invokes the Trinity � healing, cursing, and smiting in one act!");

                user.HealCombatant(Mathf.RoundToInt(user.GetEffectiveHealPower(user.CurrentHealPower)));

                target.AddEffect(new Effect(EffectType.HealingWeaken, 2, true, 3));
                target.AddEffect(new Effect(EffectType.DefenceDown, 2, true, 3));

                int baseDamage = Mathf.RoundToInt(user.GetEffectiveSkillDamage(user.CurrentSkillDmg));

                int finalDamage = baseDamage * 4;

                var (result, DamageDone) = target.TakeDamage(finalDamage, true);

                BattleUIManager.Instance.AddLog($"{user.CharacterName} smites {target.CharacterName} dealing {DamageDone} damage!");

                break;
        }

        ritualStages[user] = (stage + 1) % 3;

        return true;
    }
}
