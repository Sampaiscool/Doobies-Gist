using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectGroup
{
    None,
    BurnLike,
    PoisonLike,
    // add more groups as needed
}

public static class EffectGroupMapper
{
    public static EffectGroup ToGroup(this EffectType type)
    {
        var name = type.ToString();
        if (name.StartsWith("Burn")) return EffectGroup.BurnLike;
        if (name.StartsWith("Poison")) return EffectGroup.PoisonLike;
        return EffectGroup.None;
    }
}
