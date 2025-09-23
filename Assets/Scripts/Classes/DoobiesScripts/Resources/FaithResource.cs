using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaithResource : IResource
{
    public ResourceType Type => ResourceType.Faith;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public delegate void FaithGainHandler(int amount);
    public event FaithGainHandler OnFaithGained;

    public FaithResource(int max)
    {
        Max = max;
        Current = max;
    }

    public void Gain(int amount)
    {
        int gained = Mathf.Min(amount, Max - Current);
        if (gained <= 0) return;

        Current += gained;
        OnFaithGained?.Invoke(gained);
    }
    public void GainMax(int amount) => Max += amount;

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }
}
