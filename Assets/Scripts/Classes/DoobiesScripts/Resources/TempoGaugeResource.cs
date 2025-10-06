using UnityEngine;


[System.Serializable]
public class TempoGaugeResource : IResource
{
    public ResourceType Type => ResourceType.TempoGauge;

    public int Current { get; private set; }
    public int Max { get; private set; }
    public bool isInFocus { get; set; }

    public delegate void TempoGaugeGainHandler(int amount);
    public event TempoGaugeGainHandler OnTempoGaugeGained;

    public TempoGaugeResource(int max)
    {
        Max = max;
    }

    public void Gain(int amount)
    {
        int gained = Mathf.Min(amount, Max - Current);
        if (gained <= 0) return;

        Current += gained;
        OnTempoGaugeGained?.Invoke(gained);

        GetCurrentState();
    }
    public void GainMax(int amount) => Max += amount;

    public bool Spend(int amount)
    {
        if (Current < amount) return false;
        Current -= amount;

        GetCurrentState();

        return true;
    }
    public bool GetCurrentState()
    {
        if (Current >= (Max / 2))
        {
            isInFocus = false;
            return false;
        }
        else
        {
            isInFocus = true;
            return true;
        }
    }
}
