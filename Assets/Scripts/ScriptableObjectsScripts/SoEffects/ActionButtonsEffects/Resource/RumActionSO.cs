using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/ResourceActions/RumAction")]
public class RumActionSO : ScriptableObject, IResourceAction
{
    public string ActionName => "Make Rum";
    public void Execute(CombatantInstance user, CombatantInstance target)
    {
        if (user is DoobieInstance doobie && doobie.MainResource is RumResource rum)
        {
            int runGained = Random.Range(1, 6);
            rum.Gain(runGained);

            BattleUIManager.Instance.AddLog($"{user.CharacterName} Has made {runGained} Rum!");

            if (rum.Current >= 7)
            {
                if (rum.Current == rum.Max)
                {
                    user.AddEffect(new Effect(EffectType.DefenceDown, 5, true, 10));
                    user.AddEffect(new Effect(EffectType.WeaponStrenghten, 5, true, 10));
                    user.AddEffect(new Effect(EffectType.SpellStrenghten, 5, true, 10));
                    user.AddEffect(new Effect(EffectType.Regeneration, 5, true, 10));

                    BattleUIManager.Instance.AddLog($"{user.CharacterName} Has entered a drunken brawl!");
                }
                else
                {
                    user.AddEffect(new Effect(EffectType.DefenceDown, 5, true, 3));
                    BattleUIManager.Instance.AddLog($"{user.CharacterName} Had a little to much to drink!");
                }
            }
            else
            {
                user.AddEffect(new Effect(EffectType.Harden, 5, true, 3));
            }
        }
    }
}
