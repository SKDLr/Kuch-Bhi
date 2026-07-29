using UnityEngine;

public enum ItemType { Weapon, Armor, Consumable, Quest, Misc }

[CreateAssetMenu(menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName;
    public Sprite icon;
    [TextArea] public string description;

    [Header("Item Type")]
    public ItemType type;

    [Header("Stacking")]
    public bool isStackable;
    public int maxStack = 1;
}