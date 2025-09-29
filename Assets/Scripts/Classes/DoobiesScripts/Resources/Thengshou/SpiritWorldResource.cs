using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiritEnergyResource : IResource
{
    public ResourceType Type => ResourceType.SpiritEnergy;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public delegate void SpiritEnergyGainHandler(int amount);
    public event SpiritEnergyGainHandler OnSpiritEnergyGained;

    public SpiritEnergyResource(int max)
    {
        Max = max;
        Current = 0;
    }

    public void Gain(int amount)
    {
        int gained = Mathf.Min(amount, Max - Current);
        if (gained <= 0) return;

        Current += gained;
        OnSpiritEnergyGained?.Invoke(gained);
    }
    public void GainMax(int amount) => Max += amount;

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }
}
