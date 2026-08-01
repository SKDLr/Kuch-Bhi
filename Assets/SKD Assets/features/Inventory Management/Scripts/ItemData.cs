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

    [Header("World Prefab")]
    public GameObject worldPrefab;

    [Header("Hold Position — tweak per item in play mode")]
    public Vector3 holdOffset   = Vector3.zero;
    public Vector3 holdRotation = Vector3.zero;

    [Header("Pickup Prompt — shown when player is near")]
    public string pickupPrompt = "Press E to pick up";
}
