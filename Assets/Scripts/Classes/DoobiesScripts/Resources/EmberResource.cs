using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmberResource : IResource
{
    public ResourceType Type => ResourceType.Ember;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public delegate void EmberGainHandler(int amount);
    public event EmberGainHandler OnEmberGained;

    public EmberResource(int max)
    {
        Max = max;
        Current = 0;
    }

    public void Gain(int amount)
    {
        int gained = Mathf.Min(amount, Max - Current);
        if (gained <= 0) return;

        Current += gained;
        OnEmberGained?.Invoke(gained);
    }
    public void GainMax(int amount) => Max += amount;

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }

    public void Current5()
    {
        Current = 5;
    }
}