using System.Collections.Generic;
using UnityEngine;

public class DoobieInstance : CombatantInstance
{
    public DoobieSO _so;
    public override ScriptableObject so => _so;
    public override string CharacterName => _so.doobieName;
    public override int CurrentHealth { get; set; }
    public int currentZurp;

    public override List<SkillSO> GetAllSkills() => new List<SkillSO>(_so.baseSkills);

    public DoobieInstance(DoobieSO so)
    {
        _so = so;

        CurrentHealth = _so.hasHealth ? _so.baseHealth : -1;

        currentZurp = _so.zurp;

        EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);
    }

    public void GainZurp(int zurpGained)
    {
        currentZurp += zurpGained;

        // Zurp cannot go higher than max
        if (currentZurp >= _so.zurp)
        {
            currentZurp = _so.zurp;
        }
    }
}
