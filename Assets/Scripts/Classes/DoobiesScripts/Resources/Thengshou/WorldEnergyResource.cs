using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldEnergyResource : IResource
{
    public ResourceType Type => ResourceType.WorldEnergy;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public delegate void WorldEnergyGainHandler(int amount);
    public event WorldEnergyGainHandler OnWorldEnergyGained;

    public WorldEnergyResource(int max)
    {
        Max = max;
        Current = max / 2;
    }

    public void Gain(int amount)
    {
        int gained = Mathf.Min(amount, Max - Current);
        if (gained <= 0) return;

        Current += gained;
        OnWorldEnergyGained?.Invoke(gained);
    }
    public void GainMax(int amount) => Max += amount;

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }
}
