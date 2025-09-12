using System.Collections.Generic;
using UnityEngine;

public class DoobieInstance : CombatantInstance
{
    public DoobieSO _so;
    public override ScriptableObject so => _so;
    public override string CharacterName => _so.doobieName;
    public override int CurrentHealth { get; set; }
    public override float CurrentDefence { get; set; }

    public int currentZurp;

    public override List<SkillSO> GetAllSkills() => new List<SkillSO>(_so.baseSkills);

    public DoobieInstance(DoobieSO so)
    {
        _so = so;

        CurrentHealth = _so.hasHealth ? _so.baseHealth : -1;

        CurrentDefence = _so.baseDefence;

        currentZurp = _so.zurp;

        EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);
    }

    public void ChangeZurp(int amount, bool isZurpGain)
    {
        if (!isZurpGain)
        {
            // Zurp cannot go lower than 0
            currentZurp -= amount;
            if (currentZurp <= 0)
            {
                currentZurp = 0;
            }
            return;
        }
        else
        {
            // Zurp cannot go higher than max Zurp
            currentZurp += amount;
            if (currentZurp >= _so.zurp)
            {
                currentZurp = _so.zurp;
            }
            return;
        }
    }
}
