using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RumResource : IResource
{
    public ResourceType Type => ResourceType.Rum;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public RumResource(int max)
    {
        Max = max;
        //Current = max;
    }

    public void Gain(int amount)
    {
        Current = Mathf.Min(Current + amount, Max);
    }
    public void GainMax(int amount)
    {
        Max += amount;
    }

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }
}

