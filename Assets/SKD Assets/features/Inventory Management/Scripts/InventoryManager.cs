using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    // ── Singleton ──────────────────────────────────────────────────────────
    public static InventoryManager Instance { get; private set; }

    [Header("Settings")]
    public int maxSlots = 20;

    [Header("UI Reference")]
    public InventoryUI inventoryUI;   // drag InventoryPanel here in the Inspector

    [Header("Toggle Key")]
    public KeyCode toggleKey = KeyCode.I;

    public List<InventoryItem> items = new List<InventoryItem>();

    public event Action OnInventoryChanged;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        Debug.Log("[InventoryManager] Awake called.");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[InventoryManager] Duplicate instance found — destroying this one. " +
                             "Make sure only ONE GameManager exists in the scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        Debug.Log("[InventoryManager] Singleton set. Max slots: " + maxSlots);
    }

    // ──────────────────────────────────────────────────────────────────────
    // GameManager is ALWAYS active so this Update() always runs — safe place
    // to listen for the inventory toggle key
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (inventoryUI == null)
            {
                Debug.LogError("[InventoryManager] inventoryUI is NULL! " +
                               "Drag the InventoryPanel object into the Inventory UI field " +
                               "on the InventoryManager component in the Inspector.");
                return;
            }

            Debug.Log("[InventoryManager] Toggle key pressed — calling ToggleInventory().");
            inventoryUI.ToggleInventory();
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    public bool AddItem(ItemData data)
    {
        // Guard: null item
        if (data == null)
        {
            Debug.LogError("[InventoryManager] AddItem called with NULL ItemData! " +
                           "Check that your ItemPickup has an ItemData assigned in the Inspector.");
            return false;
        }

        Debug.Log($"[InventoryManager] Trying to add: {data.itemName} | Stackable: {data.isStackable}");

        // Try stacking
        if (data.isStackable)
        {
            InventoryItem existing = items.Find(i => i.data == data && i.quantity < data.maxStack);
            if (existing != null)
            {
                existing.AddToStack();
                Debug.Log($"[InventoryManager] Stacked {data.itemName}. New quantity: {existing.quantity}/{data.maxStack}");
                OnInventoryChanged?.Invoke();
                return true;
            }
            else
            {
                Debug.Log($"[InventoryManager] No stackable slot found for {data.itemName} — opening new slot.");
            }
        }

        // Check capacity
        if (items.Count >= maxSlots)
        {
            Debug.LogWarning($"[InventoryManager] Inventory FULL ({items.Count}/{maxSlots}). " +
                             $"Could not add {data.itemName}.");
            return false;
        }

        items.Add(new InventoryItem(data));
        Debug.Log($"[InventoryManager] Added {data.itemName} to new slot. " +
                  $"Slots used: {items.Count}/{maxSlots}");

        OnInventoryChanged?.Invoke();
        return true;
    }

    // ──────────────────────────────────────────────────────────────────────
    public void RemoveItem(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("[InventoryManager] RemoveItem called with NULL ItemData!");
            return;
        }

        InventoryItem existing = items.Find(i => i.data == data);

        if (existing == null)
        {
            Debug.LogWarning($"[InventoryManager] Tried to remove {data.itemName} but it was NOT found in inventory.");
            return;
        }

        existing.RemoveFromStack();
        Debug.Log($"[InventoryManager] Removed one {data.itemName}. Remaining: {existing.quantity}");

        if (existing.quantity <= 0)
        {
            items.Remove(existing);
            Debug.Log($"[InventoryManager] Stack depleted — slot removed. Slots used: {items.Count}/{maxSlots}");
        }

        OnInventoryChanged?.Invoke();
    }

    // ──────────────────────────────────────────────────────────────────────
    public void SwapSlots(int indexA, int indexB)
    {
        Debug.Log($"[InventoryManager] SwapSlots called: index {indexA} <-> index {indexB}");

        if (indexA < 0 || indexB < 0)
        {
            Debug.LogError($"[InventoryManager] SwapSlots — negative index! A={indexA}, B={indexB}. Aborting.");
            return;
        }

        if (indexA >= items.Count || indexB >= items.Count)
        {
            Debug.LogError($"[InventoryManager] SwapSlots — index out of range! " +
                           $"A={indexA}, B={indexB}, List size={items.Count}. Aborting.");
            return;
        }

        InventoryItem temp = items[indexA];
        items[indexA] = items[indexB];
        items[indexB] = temp;

        Debug.Log($"[InventoryManager] Swapped: {items[indexB]?.data?.itemName ?? "empty"} " +
                  $"<-> {items[indexA]?.data?.itemName ?? "empty"}");

        OnInventoryChanged?.Invoke();
    }

    // ──────────────────────────────────────────────────────────────────────
    // Public wrapper so outside classes can fire OnInventoryChanged without direct event access
    public void NotifyInventoryChanged()
    {
        Debug.Log("[InventoryManager] NotifyInventoryChanged called.");
        OnInventoryChanged?.Invoke();
    }

    // ──────────────────────────────────────────────────────────────────────
    public bool HasItem(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError("[InventoryManager] HasItem called with NULL ItemData!");
            return false;
        }
        bool result = items.Exists(i => i.data == data);
        Debug.Log($"[InventoryManager] HasItem({data.itemName}): {result}");
        return result;
    }

    // ── Print full inventory to console (call manually when debugging) ──────
    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        Debug.Log($"[InventoryManager] ===== INVENTORY ({items.Count}/{maxSlots} slots) =====");
        if (items.Count == 0)
        {
            Debug.Log("[InventoryManager] Inventory is empty.");
            return;
        }
        for (int i = 0; i < items.Count; i++)
            Debug.Log($"[InventoryManager]  Slot {i}: {items[i].data.itemName} x{items[i].quantity}");
        Debug.Log("[InventoryManager] =============================================");
    }
}































































































// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public class InventoryManager : MonoBehaviour
// {
//     // ── Singleton ──────────────────────────────────────────────────────────
//     public static InventoryManager Instance { get; private set; }

//     [Header("Settings")]
//     public int maxSlots = 20;

//     public List<InventoryItem> items = new List<InventoryItem>();

//     public event Action OnInventoryChanged;

//     // ──────────────────────────────────────────────────────────────────────
//     void Awake()
//     {
//         Debug.Log("[InventoryManager] Awake called.");

//         if (Instance != null && Instance != this)
//         {
//             Debug.LogWarning("[InventoryManager] Duplicate instance found — destroying this one. " +
//                              "Make sure only ONE GameManager exists in the scene.");
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;
//         DontDestroyOnLoad(gameObject);
//         Debug.Log("[InventoryManager] Singleton set. Max slots: " + maxSlots);
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     public bool AddItem(ItemData data)
//     {
//         // Guard: null item
//         if (data == null)
//         {
//             Debug.LogError("[InventoryManager] AddItem called with NULL ItemData! " +
//                            "Check that your ItemPickup has an ItemData assigned in the Inspector.");
//             return false;
//         }

//         Debug.Log($"[InventoryManager] Trying to add: {data.itemName} | Stackable: {data.isStackable}");

//         // Try stacking
//         if (data.isStackable)
//         {
//             InventoryItem existing = items.Find(i => i.data == data && i.quantity < data.maxStack);
//             if (existing != null)
//             {
//                 existing.AddToStack();
//                 Debug.Log($"[InventoryManager] Stacked {data.itemName}. New quantity: {existing.quantity}/{data.maxStack}");
//                 OnInventoryChanged?.Invoke();
//                 return true;
//             }
//             else
//             {
//                 Debug.Log($"[InventoryManager] No stackable slot found for {data.itemName} — opening new slot.");
//             }
//         }

//         // Check capacity
//         if (items.Count >= maxSlots)
//         {
//             Debug.LogWarning($"[InventoryManager] Inventory FULL ({items.Count}/{maxSlots}). " +
//                              $"Could not add {data.itemName}.");
//             return false;
//         }

//         items.Add(new InventoryItem(data));
//         Debug.Log($"[InventoryManager] Added {data.itemName} to new slot. " +
//                   $"Slots used: {items.Count}/{maxSlots}");

//         OnInventoryChanged?.Invoke();
//         return true;
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     public void RemoveItem(ItemData data)
//     {
//         if (data == null)
//         {
//             Debug.LogError("[InventoryManager] RemoveItem called with NULL ItemData!");
//             return;
//         }

//         InventoryItem existing = items.Find(i => i.data == data);

//         if (existing == null)
//         {
//             Debug.LogWarning($"[InventoryManager] Tried to remove {data.itemName} but it was NOT found in inventory.");
//             return;
//         }

//         existing.RemoveFromStack();
//         Debug.Log($"[InventoryManager] Removed one {data.itemName}. Remaining: {existing.quantity}");

//         if (existing.quantity <= 0)
//         {
//             items.Remove(existing);
//             Debug.Log($"[InventoryManager] Stack depleted — slot removed. Slots used: {items.Count}/{maxSlots}");
//         }

//         OnInventoryChanged?.Invoke();
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     public void SwapSlots(int indexA, int indexB)
//     {
//         Debug.Log($"[InventoryManager] SwapSlots called: index {indexA} <-> index {indexB}");

//         if (indexA < 0 || indexB < 0)
//         {
//             Debug.LogError($"[InventoryManager] SwapSlots — negative index! A={indexA}, B={indexB}. Aborting.");
//             return;
//         }

//         if (indexA >= items.Count || indexB >= items.Count)
//         {
//             Debug.LogError($"[InventoryManager] SwapSlots — index out of range! " +
//                            $"A={indexA}, B={indexB}, List size={items.Count}. Aborting.");
//             return;
//         }

//         InventoryItem temp = items[indexA];
//         items[indexA] = items[indexB];
//         items[indexB] = temp;

//         Debug.Log($"[InventoryManager] Swapped: {items[indexB]?.data?.itemName ?? "empty"} " +
//                   $"<-> {items[indexA]?.data?.itemName ?? "empty"}");

//         OnInventoryChanged?.Invoke();
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     public bool HasItem(ItemData data)
//     {
//         if (data == null)
//         {
//             Debug.LogError("[InventoryManager] HasItem called with NULL ItemData!");
//             return false;
//         }
//         bool result = items.Exists(i => i.data == data);
//         Debug.Log($"[InventoryManager] HasItem({data.itemName}): {result}");
//         return result;
//     }

//     // ── Print full inventory to console (call manually when debugging) ──────
//     [ContextMenu("Print Inventory")]
//     public void PrintInventory()
//     {
//         Debug.Log($"[InventoryManager] ===== INVENTORY ({items.Count}/{maxSlots} slots) =====");
//         if (items.Count == 0)
//         {
//             Debug.Log("[InventoryManager] Inventory is empty.");
//             return;
//         }
//         for (int i = 0; i < items.Count; i++)
//             Debug.Log($"[InventoryManager]  Slot {i}: {items[i].data.itemName} x{items[i].quantity}");
//         Debug.Log("[InventoryManager] =============================================");
//     }
// }