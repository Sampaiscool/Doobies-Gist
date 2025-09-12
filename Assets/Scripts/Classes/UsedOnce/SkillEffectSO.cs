using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SkillEffectSO : ScriptableObject
{
    // user and target are both CombatantInstances (Doobie or Vangurr)
    public abstract string ApplyEffect(CombatantInstance user, CombatantInstance target);
}
