using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;
    public string description;
    public int cost;
    public ItemType type;
    public CharacterPool Pool;
    public Sprite icon;

    public Item(string name, string desc, int cost, ItemType type, CharacterPool pool)
    {
        this.itemName = name;
        this.description = desc;
        this.cost = cost;
        this.type = type;
        Pool = pool;
    }
}
