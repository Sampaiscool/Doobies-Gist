using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ZurpResource : IResource
{
    public ResourceType Type => ResourceType.Zurp;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public delegate void ZurpGainHandler(int amount);
    public event ZurpGainHandler OnZurpGained;

    public ZurpResource(int max)
    {
        Max = max;
        Current = max;
    }

    public void Gain(int amount)
    {
        int gained = Mathf.Min(amount, Max - Current);
        if (gained <= 0) return;

        Current += gained;
        OnZurpGained?.Invoke(gained);
    }
    public void GainMax(int amount) => Max += amount;

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }
}

