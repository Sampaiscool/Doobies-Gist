using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CrystalResource : IResource
{
    public ResourceType Type => ResourceType.Crystals;

    public int Current { get; private set; }
    public int Max { get; private set; }

    public delegate void CrystalGainHandler(int amount);
    public event CrystalGainHandler OnCrystalGained;

    public CrystalResource(int max)
    {
        Max = max;
    }

    public void Gain(int amount)
    {
        int gained = Mathf.Min(amount, Max - Current);
        if (gained <= 0) return;

        Current += gained;
        OnCrystalGained?.Invoke(gained);
    }

    public void GainMax(int amount) => Max += amount;

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;
        return true;
    }
}