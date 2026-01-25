using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LocationEffectSO : ScriptableObject, ILocationEffect
{
    public abstract void ApplyEffect();
}