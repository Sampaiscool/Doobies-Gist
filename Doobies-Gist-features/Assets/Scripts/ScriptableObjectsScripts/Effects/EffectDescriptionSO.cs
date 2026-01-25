using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/EffectDescription")]
public class EffectDescriptionSO : ScriptableObject
{
    public string Name;

    [TextArea]
    public string Description;

    public EffectType EffectType;

    public Sprite Icon;

    public bool isDebuff;
}
