using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public GameObject slotPrefab;
    public Transform  slotGrid;

    // Toggle key removed from here — it lives on GameManager now
    // because SetActive(false) stops Update() from running on this object

    private List<InventorySlot> _slots = new List<InventorySlot>();
    private bool _isOpen = false;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        Debug.Log("[InventoryUI] Start called.");

        if (slotPrefab == null)
            Debug.LogError("[InventoryUI] slotPrefab is NULL! " +
                           "Drag your Slot prefab into the InventoryUI component in the Inspector.");

        if (slotGrid == null)
            Debug.LogError("[InventoryUI] slotGrid is NULL! " +
                           "Drag the SlotGrid object into the InventoryUI component in the Inspector.");

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[InventoryUI] InventoryManager.Instance is NULL! " +
                           "Make sure a GameManager with InventoryManager.cs exists in the scene " +
                           "and loads BEFORE InventoryUI (check Script Execution Order).");
            return;
        }

        InventoryManager.Instance.OnInventoryChanged += Refresh;
        Debug.Log("[InventoryUI] Subscribed to OnInventoryChanged.");

        GenerateSlots();

        // Hide the panel — GameManager.Update() will toggle it via ToggleInventory()
        gameObject.SetActive(false);
        Debug.Log("[InventoryUI] Panel hidden at start. GameManager will handle the toggle key.");
    }

    // ──────────────────────────────────────────────────────────────────────
    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
            Debug.Log("[InventoryUI] Unsubscribed from OnInventoryChanged.");
        }
    }

    // Called by GameManager every frame — lives here so UI logic stays together
    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        gameObject.SetActive(_isOpen);
        Debug.Log("[InventoryUI] Inventory panel is now: " + (_isOpen ? "OPEN" : "CLOSED"));

        if (_isOpen) Refresh();
    }

    // ──────────────────────────────────────────────────────────────────────
    private void GenerateSlots()
    {
        if (slotPrefab == null || slotGrid == null)
        {
            Debug.LogError("[InventoryUI] GenerateSlots aborted — slotPrefab or slotGrid is NULL.");
            return;
        }

        int count = InventoryManager.Instance.maxSlots;
        Debug.Log($"[InventoryUI] Generating {count} slots...");

        for (int i = 0; i < count; i++)
        {
            GameObject go = Instantiate(slotPrefab, slotGrid);
            go.name = $"Slot_{i}";

            InventorySlot slot = go.GetComponent<InventorySlot>();
            if (slot == null)
            {
                Debug.LogError($"[InventoryUI] Slot_{i} prefab is missing the InventorySlot component! " +
                               "Add InventorySlot.cs to your Slot prefab.");
                continue;
            }

            _slots.Add(slot);
        }

        Debug.Log($"[InventoryUI] {_slots.Count} slots generated successfully.");
    }

    // ──────────────────────────────────────────────────────────────────────
    private void Refresh()
    {
        List<InventoryItem> items = InventoryManager.Instance.items;
        Debug.Log($"[InventoryUI] Refresh called. Items: {items.Count} | Slots: {_slots.Count}");

        if (_slots.Count == 0)
        {
            Debug.LogWarning("[InventoryUI] Refresh called but no slots exist yet! " +
                             "GenerateSlots() may not have run.");
            return;
        }

        for (int i = 0; i < _slots.Count; i++)
        {
            if (_slots[i] == null)
            {
                Debug.LogWarning($"[InventoryUI] Slot at index {i} is NULL — it may have been destroyed.");
                continue;
            }

            if (i < items.Count)
                _slots[i].SetItem(items[i], i);
            else
                _slots[i].ClearSlot();
        }
    }
}


































































































// using System.Collections.Generic;
// using UnityEngine;

// public class InventoryUI : MonoBehaviour
// {
//     [Header("References")]
//     public GameObject slotPrefab;
//     public Transform  slotGrid;

//     [Header("Toggle Key")]
//     public KeyCode toggleKey = KeyCode.I;

//     private List<InventorySlot> _slots = new List<InventorySlot>();
//     private bool _isOpen = false;

//     // ──────────────────────────────────────────────────────────────────────
//     void Start()
//     {
//         Debug.Log("[InventoryUI] Start called.");

//         // Guard: missing references
//         if (slotPrefab == null)
//             Debug.LogError("[InventoryUI] slotPrefab is NULL! " +
//                            "Drag your Slot prefab into the InventoryUI component in the Inspector.");

//         if (slotGrid == null)
//             Debug.LogError("[InventoryUI] slotGrid is NULL! " +
//                            "Drag the SlotGrid object into the InventoryUI component in the Inspector.");

//         if (InventoryManager.Instance == null)
//         {
//             Debug.LogError("[InventoryUI] InventoryManager.Instance is NULL! " +
//                            "Make sure a GameManager with InventoryManager.cs exists in the scene " +
//                            "and loads BEFORE InventoryUI (check Script Execution Order).");
//             return;
//         }

//         InventoryManager.Instance.OnInventoryChanged += Refresh;
//         Debug.Log("[InventoryUI] Subscribed to OnInventoryChanged.");

//         GenerateSlots();

//         gameObject.SetActive(false);
//         Debug.Log("[InventoryUI] Panel hidden at start. Press [" + toggleKey + "] to open.");
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     void OnDestroy()
//     {
//         if (InventoryManager.Instance != null)
//         {
//             InventoryManager.Instance.OnInventoryChanged -= Refresh;
//             Debug.Log("[InventoryUI] Unsubscribed from OnInventoryChanged.");
//         }
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     void Update()
//     {
//         if (Input.GetKeyDown(toggleKey))
//             ToggleInventory();
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     public void ToggleInventory()
//     {
//         _isOpen = !_isOpen;
//         gameObject.SetActive(_isOpen);
//         Debug.Log("[InventoryUI] Inventory panel is now: " + (_isOpen ? "OPEN" : "CLOSED"));

//         if (_isOpen) Refresh();
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     private void GenerateSlots()
//     {
//         if (slotPrefab == null || slotGrid == null)
//         {
//             Debug.LogError("[InventoryUI] GenerateSlots aborted — slotPrefab or slotGrid is NULL.");
//             return;
//         }

//         int count = InventoryManager.Instance.maxSlots;
//         Debug.Log($"[InventoryUI] Generating {count} slots...");

//         for (int i = 0; i < count; i++)
//         {
//             GameObject go = Instantiate(slotPrefab, slotGrid);
//             go.name = $"Slot_{i}";

//             InventorySlot slot = go.GetComponent<InventorySlot>();
//             if (slot == null)
//             {
//                 Debug.LogError($"[InventoryUI] Slot_{i} prefab is missing the InventorySlot component! " +
//                                "Add InventorySlot.cs to your Slot prefab.");
//                 continue;
//             }

//             _slots.Add(slot);
//         }

//         Debug.Log($"[InventoryUI] {_slots.Count} slots generated successfully.");
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     private void Refresh()
//     {
//         List<InventoryItem> items = InventoryManager.Instance.items;
//         Debug.Log($"[InventoryUI] Refresh called. Items: {items.Count} | Slots: {_slots.Count}");

//         if (_slots.Count == 0)
//         {
//             Debug.LogWarning("[InventoryUI] Refresh called but no slots exist yet! " +
//                              "GenerateSlots() may not have run.");
//             return;
//         }

//         for (int i = 0; i < _slots.Count; i++)
//         {
//             if (_slots[i] == null)
//             {
//                 Debug.LogWarning($"[InventoryUI] Slot at index {i} is NULL — it may have been destroyed.");
//                 continue;
//             }

//             if (i < items.Count)
//                 _slots[i].SetItem(items[i], i);
//             else
//                 _slots[i].ClearSlot();
//         }
//     }
// }