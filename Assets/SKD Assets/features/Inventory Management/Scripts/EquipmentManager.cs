using System;
using System.Collections.Generic;
using UnityEngine;

public enum EquipSlot { Head, Chest, Legs, Feet, Weapon, Offhand }

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    [Header("Hand Bone — drag mixamorig:RightHandIndex1 here")]
    public Transform rightHandBone;

    private GameObject _heldObject;
    private Dictionary<EquipSlot, ItemData> _equipped = new Dictionary<EquipSlot, ItemData>();

    public event Action OnEquipmentChanged;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
            _equipped[slot] = null;

        if (rightHandBone == null)
            Debug.LogError("[EquipmentManager] rightHandBone is NULL! " +
                           "Drag mixamorig:RightHandIndex1 into the Inspector.");
    }

    // ──────────────────────────────────────────────────────────────────────
    // Called by HotbarManager when a weapon slot is selected
    public void EquipFromHotbar(ItemData item)
    {
        if (item == null) { ClearHand(); return; }

        Debug.Log($"[EquipmentManager] Equipping from hotbar: {item.itemName}");
        _equipped[EquipSlot.Weapon] = item;
        SpawnInHand(item);
        OnEquipmentChanged?.Invoke();
    }

    // ──────────────────────────────────────────────────────────────────────
    // Remove whatever is in the hand
    public void ClearHand()
    {
        Debug.Log("[EquipmentManager] Clearing hand.");
        _equipped[EquipSlot.Weapon] = null;
        DestroyHeldObject();
        OnEquipmentChanged?.Invoke();
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Equip(ItemData item, EquipSlot slot)
    {
        if (item == null) { Debug.LogError("[EquipmentManager] Equip called with NULL!"); return; }

        if (_equipped[slot] != null)
            Unequip(slot);

        _equipped[slot] = item;
        InventoryManager.Instance.RemoveItem(item);

        if (slot == EquipSlot.Weapon)
            SpawnInHand(item);

        Debug.Log($"[EquipmentManager] {item.itemName} equipped to {slot}.");
        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(EquipSlot slot)
    {
        if (_equipped[slot] == null) return;

        string name  = _equipped[slot].itemName;
        bool   added = InventoryManager.Instance.AddItem(_equipped[slot]);

        if (added)
        {
            if (slot == EquipSlot.Weapon) DestroyHeldObject();
            _equipped[slot] = null;
            Debug.Log($"[EquipmentManager] {name} unequipped.");
            OnEquipmentChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[EquipmentManager] Cannot unequip {name} — inventory full!");
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    private void SpawnInHand(ItemData item)
    {
        if (item.worldPrefab == null)
        {
            Debug.LogWarning($"[EquipmentManager] {item.itemName} has no World Prefab — nothing shown in hand.");
            return;
        }

        if (rightHandBone == null)
        {
            Debug.LogError("[EquipmentManager] rightHandBone is NULL — cannot spawn in hand!");
            return;
        }

        DestroyHeldObject();

        _heldObject = Instantiate(item.worldPrefab, rightHandBone);
        _heldObject.transform.localPosition = item.holdOffset;
        _heldObject.transform.localRotation = Quaternion.Euler(item.holdRotation);
        _heldObject.transform.localScale    = Vector3.one;

        // Disable physics on held object
        Rigidbody rb = _heldObject.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        foreach (Collider col in _heldObject.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Disable ItemPickup so player cannot pick up their own held weapon
        // Without this the prompt shows and pressing E duplicates the item
        ItemPickup pickup = _heldObject.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.enabled = false;
            Debug.Log($"[EquipmentManager] Disabled ItemPickup on held {item.itemName}.");
        }

        // Destroy any PickupTrigger child on the prefab
        Transform triggerChild = _heldObject.transform.Find("PickupTrigger");
        if (triggerChild != null)
            Destroy(triggerChild.gameObject);

        Debug.Log($"[EquipmentManager] Spawned {item.itemName} in right hand bone.");
    }

    private void DestroyHeldObject()
    {
        if (_heldObject != null)
        {
            Destroy(_heldObject);
            _heldObject = null;
        }
    }

    public ItemData GetEquipped(EquipSlot slot) => _equipped[slot];
    public bool IsSlotFilled(EquipSlot slot)    => _equipped[slot] != null;
}










// using System;
// using System.Collections.Generic;
// using UnityEngine;

// public enum EquipSlot { Head, Chest, Legs, Feet, Weapon, Offhand }

// public class EquipmentManager : MonoBehaviour
// {
//     public static EquipmentManager Instance { get; private set; }

//     [Header("Hand Bone — drag mixamorig:RightHandIndex1 here")]
//     public Transform rightHandBone;

//     private GameObject _heldObject;
//     private Dictionary<EquipSlot, ItemData> _equipped = new Dictionary<EquipSlot, ItemData>();

//     public event Action OnEquipmentChanged;

//     // ──────────────────────────────────────────────────────────────────────
//     void Awake()
//     {
//         if (Instance != null && Instance != this) { Destroy(gameObject); return; }
//         Instance = this;

//         foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
//             _equipped[slot] = null;

//         if (rightHandBone == null)
//             Debug.LogError("[EquipmentManager] rightHandBone is NULL! " +
//                            "Drag mixamorig:RightHandIndex1 into the Inspector.");
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     // Called by HotbarManager when a weapon slot is selected
//     public void EquipFromHotbar(ItemData item)
//     {
//         if (item == null) { ClearHand(); return; }

//         Debug.Log($"[EquipmentManager] Equipping from hotbar: {item.itemName}");
//         _equipped[EquipSlot.Weapon] = item;
//         SpawnInHand(item);
//         OnEquipmentChanged?.Invoke();
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     // Remove whatever is in the hand
//     public void ClearHand()
//     {
//         Debug.Log("[EquipmentManager] Clearing hand.");
//         _equipped[EquipSlot.Weapon] = null;
//         DestroyHeldObject();
//         OnEquipmentChanged?.Invoke();
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     public void Equip(ItemData item, EquipSlot slot)
//     {
//         if (item == null) { Debug.LogError("[EquipmentManager] Equip called with NULL!"); return; }

//         if (_equipped[slot] != null)
//             Unequip(slot);

//         _equipped[slot] = item;
//         InventoryManager.Instance.RemoveItem(item);

//         if (slot == EquipSlot.Weapon)
//             SpawnInHand(item);

//         Debug.Log($"[EquipmentManager] {item.itemName} equipped to {slot}.");
//         OnEquipmentChanged?.Invoke();
//     }

//     public void Unequip(EquipSlot slot)
//     {
//         if (_equipped[slot] == null) return;

//         string name  = _equipped[slot].itemName;
//         bool   added = InventoryManager.Instance.AddItem(_equipped[slot]);

//         if (added)
//         {
//             if (slot == EquipSlot.Weapon) DestroyHeldObject();
//             _equipped[slot] = null;
//             Debug.Log($"[EquipmentManager] {name} unequipped.");
//             OnEquipmentChanged?.Invoke();
//         }
//         else
//         {
//             Debug.LogWarning($"[EquipmentManager] Cannot unequip {name} — inventory full!");
//         }
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     private void SpawnInHand(ItemData item)
//     {
//         if (item.worldPrefab == null)
//         {
//             Debug.LogWarning($"[EquipmentManager] {item.itemName} has no World Prefab — nothing shown in hand.");
//             return;
//         }

//         if (rightHandBone == null)
//         {
//             Debug.LogError("[EquipmentManager] rightHandBone is NULL — cannot spawn in hand!");
//             return;
//         }

//         DestroyHeldObject();

//         _heldObject = Instantiate(item.worldPrefab, rightHandBone);
//         _heldObject.transform.localPosition = item.holdOffset;
//         _heldObject.transform.localRotation = Quaternion.Euler(item.holdRotation);
//         _heldObject.transform.localScale    = Vector3.one;

//         // Disable physics on held object
//         Rigidbody rb = _heldObject.GetComponent<Rigidbody>();
//         if (rb != null) rb.isKinematic = true;

//         foreach (Collider col in _heldObject.GetComponentsInChildren<Collider>())
//             col.enabled = false;

//         Debug.Log($"[EquipmentManager] Spawned {item.itemName} in right hand bone.");
//     }

//     private void DestroyHeldObject()
//     {
//         if (_heldObject != null)
//         {
//             Destroy(_heldObject);
//             _heldObject = null;
//         }
//     }

//     public ItemData GetEquipped(EquipSlot slot) => _equipped[slot];
//     public bool IsSlotFilled(EquipSlot slot)    => _equipped[slot] != null;
// }
