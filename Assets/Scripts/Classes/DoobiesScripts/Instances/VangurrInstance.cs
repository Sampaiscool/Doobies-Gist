using System.Collections.Generic;
using UnityEngine;

public class VangurrInstance : CombatantInstance
{
    public VangurrSO _so;
    public override ScriptableObject so => _so;
    public override string CharacterName => _so.vangurrName;
    public override int CurrentHealth { get; set; }

    public override float CurrentDefence { get; set; }

    public override List<SkillSO> GetAllSkills() => new List<SkillSO>(_so.baseSkills);

    public VangurrInstance(VangurrSO so)
    {
        _so = so;
        CurrentHealth = _so.baseHealth;
        CurrentDefence = _so.baseDefence;
        EquippedWeaponInstance = new WeaponInstance(_so.defaultWeapon);
    }
}
