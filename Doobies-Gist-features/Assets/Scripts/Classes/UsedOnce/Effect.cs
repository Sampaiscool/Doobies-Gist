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

    public Effect(EffectType type, int duration, bool isDebuff, int intensity = 1)
    {
        this.type = type;
        this.duration = duration;
        this.isDebuff = isDebuff;
        this.intensity = intensity;
    }
}
