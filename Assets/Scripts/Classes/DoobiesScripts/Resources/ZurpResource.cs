using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZurpResource : IResource
{
    public ResourceType Type => ResourceType.Zurp;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public ZurpResource(int max)
    {
        Max = max;
        Current = max;
    }

    public void Gain(int amount)
    {
        Current = Mathf.Min(Current + amount, Max);
    }

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }
}

