using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Debuff
{
    public DebuffType type;
    public int duration; // in turns
    public int intensity; // optional, for effects like poison damage per turn

    public Debuff(DebuffType type, int duration, int intensity = 1)
    {
        this.type = type;
        this.duration = duration;
        this.intensity = intensity;
    }
}