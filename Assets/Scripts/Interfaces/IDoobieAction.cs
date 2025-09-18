using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDoobieAction
{
    string ActionName { get; }                // what shows on the button
    void Execute(CombatantInstance user, CombatantInstance target); // what happens when pressed
}
