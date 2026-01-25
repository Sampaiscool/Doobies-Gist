using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageResult
{
    Hit,
    Missed,       // for future dodges, invisibility, etc.
    Deflected,
    Immune,       // in case something shrugs off damage
    Blocked ,      // optional shield-style effect
    Dodged        // when evasion buff is active
}
