using System;
using UnityEngine;

// Hotbar now owns its items — moving to hotbar removes from inventory bag
public class HotbarManager : MonoBehaviour
{
    public static HotbarManager Instance { get; private set; }
    public const int SLOT_COUNT = 5;

    // Hotbar stores full InventoryItem so stacks work too
    private InventoryItem[] _slots = new InventoryItem[SLOT_COUNT];
    private int _selected = 0;

    public event Action      OnHotbarChanged;
    public event Action<int> OnSelectionChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Update()
    {
        if (InventoryUI.Instance != null && InventoryUI.Instance.IsOpen) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) SelectSlot(_selected - 1);
        if (scroll < 0f) SelectSlot(_selected + 1);

        for (int i = 0; i < SLOT_COUNT; i++)
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                SelectSlot(i);
    }

    public void SelectSlot(int index)
    {
        _selected = (index % SLOT_COUNT + SLOT_COUNT) % SLOT_COUNT;
        Debug.Log($"[HotbarManager] Selected slot {_selected}");
        OnSelectionChanged?.Invoke(_selected);
        ApplyEquip();
    }

    void ApplyEquip()
    {
        InventoryItem item = _slots[_selected];
        if (item != null && item.data.type == ItemType.Weapon)
            EquipmentManager.Instance?.EquipFromHotbar(item.data);
        else
            EquipmentManager.Instance?.ClearHand();
    }

    // ── Move item FROM inventory INTO hotbar slot ──────────────────────────
    public bool MoveFromInventory(InventoryItem invItem, int hotbarIndex)
    {
        if (invItem == null || hotbarIndex < 0 || hotbarIndex >= SLOT_COUNT) return false;

        // If hotbar slot already has something, send it back to inventory first
        if (_slots[hotbarIndex] != null)
            ReturnToInventory(hotbarIndex);

        // Remove from inventory
        InventoryManager.Instance.items.Remove(invItem);
        _slots[hotbarIndex] = invItem;

        Debug.Log($"[HotbarManager] Moved {invItem.data.itemName} from inventory to hotbar slot {hotbarIndex}.");
        InventoryManager.Instance.NotifyInventoryChanged();
        OnHotbarChanged?.Invoke();

        if (hotbarIndex == _selected) ApplyEquip();
        return true;
    }

    // ── Move item FROM hotbar back to inventory ────────────────────────────
    public bool ReturnToInventory(int hotbarIndex)
    {
        if (hotbarIndex < 0 || hotbarIndex >= SLOT_COUNT) return false;
        if (_slots[hotbarIndex] == null) return false;

        InventoryItem item = _slots[hotbarIndex];

        if (InventoryManager.Instance.items.Count >= InventoryManager.Instance.maxSlots)
        {
            Debug.LogWarning("[HotbarManager] Inventory full — can't return item.");
            return false;
        }

        InventoryManager.Instance.items.Add(item);
        _slots[hotbarIndex] = null;

        Debug.Log($"[HotbarManager] Returned {item.data.itemName} from hotbar slot {hotbarIndex} to inventory.");
        InventoryManager.Instance.NotifyInventoryChanged();
        OnHotbarChanged?.Invoke();

        if (hotbarIndex == _selected) ApplyEquip();
        return true;
    }

    // ── Swap two hotbar slots ──────────────────────────────────────────────
    public void SwapHotbarSlots(int a, int b)
    {
        if (a < 0 || b < 0 || a >= SLOT_COUNT || b >= SLOT_COUNT) return;
        InventoryItem temp = _slots[a];
        _slots[a] = _slots[b];
        _slots[b] = temp;
        Debug.Log($"[HotbarManager] Swapped hotbar slots {a} <-> {b}");
        OnHotbarChanged?.Invoke();
        if (a == _selected || b == _selected) ApplyEquip();
    }

    // ── Drop item from hotbar into world ───────────────────────────────────
    public void DropFromHotbar(int hotbarIndex)
    {
        if (_slots[hotbarIndex] == null) return;
        InventoryItem item = _slots[hotbarIndex];
        _slots[hotbarIndex] = null;
        OnHotbarChanged?.Invoke();
        if (hotbarIndex == _selected) ApplyEquip();
        ItemDropper.Instance?.DropItemDirect(item.data);
        Debug.Log($"[HotbarManager] Dropped {item.data.itemName} from hotbar slot {hotbarIndex}.");
    }

    // ── Clear slot without returning to inventory (for drop/destroy) ────────
    public void ClearSlot(int index)
    {
        if (index < 0 || index >= SLOT_COUNT) return;
        _slots[index] = null;
        OnHotbarChanged?.Invoke();
        if (index == _selected) ApplyEquip();
    }

    public InventoryItem GetSlot(int index) => (index >= 0 && index < SLOT_COUNT) ? _slots[index] : null;
    public int           GetSelected()      => _selected;

    // Public wrapper so outside classes can fire OnHotbarChanged safely
    public void NotifyHotbarChanged()
    {
        OnHotbarChanged?.Invoke();
    }
}
