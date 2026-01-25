using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Locations/Location")]
public class LocationSO : ScriptableObject
{
    public string locationName;
    public Sprite locationImage;
    public int locationChance;

    [TextArea(1, 5)]
    public string description;

    public LocationEffectSO effect;
}
