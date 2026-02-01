using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Effect
{
    public EffectType type;
    public int duration;
    public int intensity;
    public bool isDebuff;

    [HideInInspector]
    public EffectIcon iconInstance; // track the UI

    // Optional: store the icon reference for later
    [System.NonSerialized]
    public GameObject iconGO;

    // Optional: when an effect is applied by someone, they can stamp their burn level
    // so the receiver can use the source's burn level instead of their own.
    [System.NonSerialized]
    public int? sourceBurnLevel = null;

    // Optional: reference to the combatant who created/cast this effect
    // Used to automatically apply the source's burn level to burn effects
    [System.NonSerialized]
    public CombatantInstance sourceCombatant = null;

    public Effect(EffectType type, int duration, bool isDebuff, int intensity = 1)
    {
        this.type = type;
        this.duration = duration;
        this.isDebuff = isDebuff;
        this.intensity = intensity;
    }
}
