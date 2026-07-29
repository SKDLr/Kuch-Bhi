using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public ItemData data;
    public int quantity;

    public InventoryItem(ItemData itemData)
    {
        if (itemData == null)
            Debug.LogError("[InventoryItem] Created with NULL ItemData! Check your ScriptableObject assignment.");

        data = itemData;
        quantity = 1;
        Debug.Log($"[InventoryItem] New instance created: {data?.itemName ?? "NULL"} x1");
    }

    public void AddToStack()
    {
        quantity++;
        Debug.Log($"[InventoryItem] AddToStack: {data?.itemName ?? "NULL"} is now x{quantity}");
    }

    public void RemoveFromStack()
    {
        quantity--;
        Debug.Log($"[InventoryItem] RemoveFromStack: {data?.itemName ?? "NULL"} is now x{quantity}");

        if (quantity < 0)
            Debug.LogError($"[InventoryItem] Quantity went NEGATIVE ({quantity}) for {data?.itemName}! " +
                           "Something removed more than was available.");
    }
}