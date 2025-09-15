using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IResource
{
    ResourceType Type { get; }
    int Current { get; }
    int Max { get; }

    void Gain(int amount);
    bool Spend(int amount);
}
