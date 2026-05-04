using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill/SkillSO")]
public class SkillSO : ScriptableObject
{
    /// <summary>
    /// Name of the skill
    /// </summary>
    public string skillName;
    /// <summary>
    /// Sprite of the skill
    /// </summary>
    public Sprite icon;
    /// <summary>
    /// Animation prefab of the skill 
    /// </summary>
    public GameObject animation;
    /// <summary>
    /// Description of the skill
    /// </summary>
    [TextArea]
    public string description;
    /// <summary>
    /// Resource amount
    /// </summary>
    public int resourceCost;
    /// <summary>
    /// The resource the skill uses
    /// </summary>
    public ResourceType resourceUsed;
    /// <summary>
    /// Whether the skill is a weapon-stle
    /// </summary>
    public bool isWeaponSkill;
    /// <summary>
    /// Chance to regain zurp (0-1)
    /// </summary>
    public float zurpRegainChance;
    /// <summary>
    /// The zurp you regain after using the skill
    /// </summary>
    public int zurpRegainAmount;
    /// <summary>
    /// The skills own effect SO
    /// </summary>
    public SkillEffectSO effect;
    /// <summary>
    /// Uses the skill and pays its cost
    /// </summary>
    /// <remarks>Also proc zurp regain</remarks>
    /// <param name="user">The Instance that uses the skill</param>
    /// <param name="target">The target of the skill</param>
    /// <returns>The string that BattleUIManager needs</returns>
    public string UseSkill(CombatantInstance user, CombatantInstance target)
    {
        if (effect == null)
            return $"{skillName} fizzles into the void...";

        var userEffectsSnapshot = new List<Effect>(user.ActiveEffects);

        if (user is DoobieInstance doobie)
        {
            if (resourceUsed == ResourceType.Health)
            {
                doobie.CurrentHealth -= resourceCost;
            }
            else if (resourceUsed == ResourceType.WorldEnergy)
            {
                if (doobie.MainResource is SoulflowResource soulflow)
                {
                    bool success = soulflow.WorldEnergy.Spend(resourceCost);
                    if (!success)
                        return $"{doobie.CharacterName} tried to cast {skillName}, but lacked enough {resourceUsed}!";
                }
            }
            else if (resourceUsed == ResourceType.SpiritEnergy)
            {
                if (doobie.MainResource is SoulflowResource soulflow)
                {
                    bool success = soulflow.SpiritEnergy.Spend(resourceCost);
                    if (!success)
                        return $"{doobie.CharacterName} tried to cast {skillName}, but lacked enough {resourceUsed}!";
                }
            }
            else if (doobie.MainResource != null && doobie.MainResource.Type == resourceUsed)
            {
                bool success = doobie.MainResource.Spend(resourceCost);
                if (!success)
                    return $"{doobie.CharacterName} tried to cast {skillName}, but lacked enough {resourceUsed}!";
            }
        }

        target.PlayAttackAnimation(animation);

        string result = effect.ApplyEffect(user, target, this);

        foreach (Effect activeEffect in userEffectsSnapshot)
        {
            if (activeEffect.type == EffectType.Shadow)
            {
                int castAmount = activeEffect.intensity;

                for (int i = 0; i < castAmount; i++)
                {
                    BattleUIManager.Instance.AddLog($"{effect.ApplyEffect(user, target, this)}");
                }

                user.ActiveEffects.Remove(activeEffect);
            }
        }

        if (isWeaponSkill)
            user.CheckForWeaponOnUseEffects();
        else
            user.CheckForSpelllOnUseEffects();

        user.CheckForAttackEffects();

        user.CheckForSkillOnUseEffects();

        

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
