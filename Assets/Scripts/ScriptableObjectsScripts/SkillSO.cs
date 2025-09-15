using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/SkillSO")]
public class SkillSO : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public GameObject animation;
    public string description;
    public int resourceCost;
    public ResourceType resourceUsed; // HP or Zurp
    public bool isWeaponSkill; // If true, damage is based on weapon. If false, damage is based on skillDmg.
    public float zurpRegainChance;
    public int zurpRegainAmount;

    public SkillEffectSO effect;

    public string UseSkill(CombatantInstance user, CombatantInstance target)
    {
        if (effect == null)
            return $"{skillName} fizzles into the void...";

        if (user is DoobieInstance doobie)
        {
            if (resourceUsed == ResourceType.Health)
            {
                doobie.CurrentHealth -= resourceCost;
            }
            else if (doobie.MainResource != null && doobie.MainResource.Type == resourceUsed)
            {
                bool success = doobie.MainResource.Spend(resourceCost);
                if (!success)
                    return $"{doobie.CharacterName} tried to cast {skillName}, but lacked enough {resourceUsed}!";
            }
        }

        string result = effect.ApplyEffect(user, target);

        if (isWeaponSkill)
            user.CheckForWeaponOnUseEffects();
        else
            user.CheckForSkillOnUseEffects();

        target.PlayAttackAnimation(animation);

        // Resource restore (only if matches MainResource)
        if (user is DoobieInstance spellcaster && spellcaster.MainResource?.Type == ResourceType.Zurp)
        {
            if (Random.value < zurpRegainChance)
            {
                spellcaster.MainResource.Gain(zurpRegainAmount);
                Debug.Log($"{spellcaster.CharacterName} regains {zurpRegainAmount} Zurp from casting {skillName}!");
                result += $"\nYou also regain {zurpRegainAmount} zurp!";
            }
        }

        return result;
    }
}
