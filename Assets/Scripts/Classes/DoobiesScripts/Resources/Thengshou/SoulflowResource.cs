using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoulflowResource : IResource
{
    public ResourceType Type => ResourceType.Soulflow;
    public int Current => WorldEnergy.Current + SpiritEnergy.Current;
    public int Max => WorldEnergy.Max + SpiritEnergy.Max;

    public WorldEnergyResource WorldEnergy { get; private set; }
    public SpiritEnergyResource SpiritEnergy { get; private set; }

    public delegate void SoulflowGainHandler(int amount);
    public event SoulflowGainHandler OnSoulflowGained;

    public SoulflowResource(int baseMax)
    {
        WorldEnergy = new WorldEnergyResource(baseMax);
        SpiritEnergy = new SpiritEnergyResource(baseMax);
    }

    public void Gain(int amount)
    {
        int half = Mathf.CeilToInt(amount / 2f);
        WorldEnergy.Gain(half);
        SpiritEnergy.Gain(amount - half);

        OnSoulflowGained?.Invoke(amount);
    }

    public void GainMax(int amount)
    {
        WorldEnergy.GainMax(amount);
        SpiritEnergy.GainMax(amount);
    }

    public bool Spend(int amount)
    {
        // Default: try world first, then spirit
        if (WorldEnergy.Current >= amount)
            return WorldEnergy.Spend(amount);

        if (SpiritEnergy.Current >= amount)
            return SpiritEnergy.Spend(amount);

        return false;
    }
}

