using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDoobieAction
{
    string ActionName { get; }                // what shows on the button
    string Description { get; }
    bool Execute(CombatantInstance user, CombatantInstance target); // what happens when pressed
}
