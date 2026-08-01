using UnityEngine;

// Attach to GameManager
// Handles using/consuming items from hotbar
// When you build health system, fill in the Use() cases below

public class ItemUsageHandler : MonoBehaviour
{
    public static ItemUsageHandler Instance { get; private set; }

    [Header("Use Key")]
    public KeyCode useKey = KeyCode.F;  // press F to use selected hotbar item

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Update()
    {
        // Only use items when inventory is closed
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen) return;

        if (Input.GetKeyDown(useKey))
            UseSelectedHotbarItem();
    }

    // ──────────────────────────────────────────────────────────────────────
    public void UseSelectedHotbarItem()
    {
        int selected = HotbarManager.Instance.GetSelected();
        InventoryItem item = HotbarManager.Instance.GetSlot(selected);

        if (item == null)
        {
            Debug.Log("[ItemUsageHandler] No item in selected hotbar slot.");
            return;
        }

        Debug.Log($"[ItemUsageHandler] Using {item.data.itemName} x{item.quantity}");
        UseItem(item, selected);
    }

    // ──────────────────────────────────────────────────────────────────────
    void UseItem(InventoryItem item, int hotbarSlot)
    {
        switch (item.data.type)
        {
            case ItemType.Consumable:
                ConsumeItem(item, hotbarSlot);
                break;

            case ItemType.Weapon:
                Debug.Log($"[ItemUsageHandler] {item.data.itemName} is a weapon — already equipped by hotbar selection.");
                break;

            default:
                Debug.Log($"[ItemUsageHandler] {item.data.itemName} ({item.data.type}) has no use action yet.");
                break;
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    void ConsumeItem(InventoryItem item, int hotbarSlot)
    {
        Debug.Log($"[ItemUsageHandler] Consuming {item.data.itemName}. Stack before: {item.quantity}");

        // ── Health restore ─────────────────────────────────────────────────
        if (item.data.healAmount > 0f)
        {
            if (PlayerHealth.Instance != null)
            {
                PlayerHealth.Instance.Heal(item.data.healAmount);
                Debug.Log($"[ItemUsageHandler] Healed {item.data.healAmount} HP from {item.data.itemName}.");
            }
            else
            {
                Debug.LogWarning("[ItemUsageHandler] PlayerHealth.Instance is NULL — " +
                                 "make sure PlayerHealth.cs is on the Player.");
            }
        }
        else
        {
            Debug.Log($"[ItemUsageHandler] {item.data.itemName} consumed but healAmount is 0. " +
                      "Set Heal Amount in the ItemData Inspector.");
        }
        // ──────────────────────────────────────────────────────────────────

        // Decrement stack by 1
        item.RemoveFromStack();

        if (item.quantity <= 0)
        {
            // Stack empty — clear the hotbar slot
            HotbarManager.Instance.ClearSlot(hotbarSlot);
            Debug.Log($"[ItemUsageHandler] {item.data.itemName} stack depleted — hotbar slot {hotbarSlot} cleared.");
        }
        else
        {
            // Still has quantity — refresh hotbar UI to show updated count
            HotbarManager.Instance.NotifyHotbarChanged();
            Debug.Log($"[ItemUsageHandler] {item.data.itemName} remaining: {item.quantity}");
        }
    }
}
