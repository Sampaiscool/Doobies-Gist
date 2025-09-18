using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResourceAction
{
    string ActionName { get; }
    void Execute(CombatantInstance user, CombatantInstance target);
}

