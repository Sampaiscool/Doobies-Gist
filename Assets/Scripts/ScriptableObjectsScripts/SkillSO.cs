using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Skill")]
public class SkillSO : ScriptableObject
{
    public string skillName;
    public Sprite icon;
    public GameObject animation;
    public string description;
    public int resourceCost;
    public ResourceType resourceUsed; // HP or Zurp
    public Debuff debuffToApply;
    public float zurpRegainChance;
    public int zurpRegainAmount;

    public SkillEffectSO effect;

    public string UseSkill(CombatantInstance user, CombatantInstance target)
    {
        if (effect == null)
            return $"{skillName} fizzles into the void...";

        // Only apply cost if user is a Doobie
        if (user is DoobieInstance doobie)
        {
            switch (resourceUsed)
            {
                case ResourceType.Health:
                    doobie.CurrentHealth -= resourceCost;
                    break;
                case ResourceType.Mana:
                    doobie.ChangeZurp(resourceCost, false);
                    break;
            }
        }
        string result = effect.ApplyEffect(user, target);

        // Roll for Zurp restore only if user is Doobie and this was a spell
        if (user is DoobieInstance spellcaster)
        {
            var doobieSO = spellcaster.so as DoobieSO;
            if (doobieSO != null && Random.value < zurpRegainChance)
            {
                spellcaster.ChangeZurp(zurpRegainAmount, true);
                Debug.Log($"{spellcaster.CharacterName} regains 1 Zurp from casting {skillName}!");
            }
        }

        return result;
    }
}
