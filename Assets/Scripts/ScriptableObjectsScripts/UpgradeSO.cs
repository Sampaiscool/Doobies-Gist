using UnityEngine;

[CreateAssetMenu(menuName = "SO/Upgrade")]
public class UpgradeSO : ScriptableObject
{
    public string upgradeName;
    [TextArea(2, 5)] public string description;
    public int cost;
    public Sprite icon;
    public UpgradeNames type;
    public CharacterPool pool;
    public int intensity;
}
