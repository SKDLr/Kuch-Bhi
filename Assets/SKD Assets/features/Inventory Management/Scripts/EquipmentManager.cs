using System;
using System.Collections.Generic;
using UnityEngine;

public enum EquipSlot { Head, Chest, Legs, Feet, Weapon, Offhand }

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance { get; private set; }

    private Dictionary<EquipSlot, ItemData> _equipped = new Dictionary<EquipSlot, ItemData>();

    public event Action OnEquipmentChanged;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        Debug.Log("[EquipmentManager] Awake called.");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[EquipmentManager] Duplicate detected — destroying this one.");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
            _equipped[slot] = null;

        Debug.Log("[EquipmentManager] All equipment slots initialized to empty.");
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Equip(ItemData item, EquipSlot slot)
    {
        if (item == null)
        {
            Debug.LogError("[EquipmentManager] Equip called with NULL item!");
            return;
        }

        Debug.Log($"[EquipmentManager] Equipping {item.itemName} to {slot}.");

        if (_equipped[slot] != null)
        {
            Debug.Log($"[EquipmentManager] {slot} already has {_equipped[slot].itemName} — unequipping first.");
            Unequip(slot);
        }

        _equipped[slot] = item;
        InventoryManager.Instance.RemoveItem(item);

        Debug.Log($"[EquipmentManager] {item.itemName} equipped to {slot} successfully.");
        OnEquipmentChanged?.Invoke();
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Unequip(EquipSlot slot)
    {
        if (_equipped[slot] == null)
        {
            Debug.LogWarning($"[EquipmentManager] Tried to unequip {slot} but it's already empty.");
            return;
        }

        string itemName = _equipped[slot].itemName;
        bool added = InventoryManager.Instance.AddItem(_equipped[slot]);

        if (added)
        {
            Debug.Log($"[EquipmentManager] {itemName} unequipped from {slot} and returned to inventory.");
            _equipped[slot] = null;
            OnEquipmentChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"[EquipmentManager] Cannot unequip {itemName} — inventory is full! " +
                             "Free up a slot first.");
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    public ItemData GetEquipped(EquipSlot slot) => _equipped[slot];

    public bool IsSlotFilled(EquipSlot slot) => _equipped[slot] != null;

    // ── Print all equipment to console ─────────────────────────────────────
    [ContextMenu("Print Equipment")]
    public void PrintEquipment()
    {
        Debug.Log("[EquipmentManager] ===== EQUIPMENT =====");
        foreach (EquipSlot slot in Enum.GetValues(typeof(EquipSlot)))
            Debug.Log($"[EquipmentManager]  {slot}: {(_equipped[slot]?.itemName ?? "empty")}");
        Debug.Log("[EquipmentManager] ====================");
    }
}