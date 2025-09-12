using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICombatantSO
{
    Sprite portrait { get; }
    int baseHealth { get; }
}
