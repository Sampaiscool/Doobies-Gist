using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item")]
public class ItemSO : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public int cost;
    public bool isLegendary;
    public ItemType type;
    public CharacterPool pool;
    public bool hasBeenPurchased;
}

public enum ItemType
{
    None,
    StrikingFlower,                   // Hiroshi
    DualBarrels,                      // Cobb Silver Eye
    BleedingSpirit,                   // Thengshou
    SharedPain,                       // HealthResource
}
