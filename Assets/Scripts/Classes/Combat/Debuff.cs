using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Buff
{
    public BuffType type;
    public int duration; // in turns
    public int intensity; // optioneel, bv. stacks
    public bool isDebuff; // true = debuff, false = buff

    public Buff(BuffType type, int duration, bool isDebuff = false, int intensity = 1)
    {
        this.type = type;
        this.duration = duration;
        this.intensity = intensity;
        this.isDebuff = isDebuff;
    }
}