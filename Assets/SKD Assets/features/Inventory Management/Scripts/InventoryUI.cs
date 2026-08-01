using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("References")]
    public GameObject       slotPrefab;
    public Transform        slotGrid;
    public PlayerController playerController;

    // Exposed so HotbarManager.Update() can pause scroll when inventory open
    public bool IsOpen => _isOpen;

    private List<InventorySlot> _slots = new List<InventorySlot>();
    private bool _isOpen = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (slotPrefab == null)        Debug.LogError("[InventoryUI] slotPrefab is NULL!");
        if (slotGrid == null)          Debug.LogError("[InventoryUI] slotGrid is NULL!");
        if (playerController == null)  Debug.LogWarning("[InventoryUI] playerController not assigned.");

        if (InventoryManager.Instance == null) { Debug.LogError("[InventoryUI] InventoryManager NULL!"); return; }

        InventoryManager.Instance.OnInventoryChanged += Refresh;
        GenerateSlots();
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
    }

    // Called by InventoryManager.Update() for I key AND by X button OnClick
    public void ToggleInventory()
    {
        _isOpen = !_isOpen;
        gameObject.SetActive(_isOpen);

        if (playerController != null)
            playerController.inventoryOpen = _isOpen;

        Cursor.lockState = _isOpen ? CursorLockMode.None   : CursorLockMode.Locked;
        Cursor.visible   = _isOpen;

        if (_isOpen) Refresh();
        Debug.Log("[InventoryUI] " + (_isOpen ? "OPEN" : "CLOSED"));
    }

    void GenerateSlots()
    {
        int count = InventoryManager.Instance.maxSlots;
        for (int i = 0; i < count; i++)
        {
            GameObject    go   = Instantiate(slotPrefab, slotGrid);
            go.name            = $"Slot_{i}";
            InventorySlot slot = go.GetComponent<InventorySlot>();
            if (slot == null) { Debug.LogError($"[InventoryUI] Slot_{i} missing InventorySlot!"); continue; }
            _slots.Add(slot);
        }
        Debug.Log($"[InventoryUI] {_slots.Count} slots generated.");
    }

    public void Refresh()
    {
        var items = InventoryManager.Instance.items;
        for (int i = 0; i < _slots.Count; i++)
        {
            if (i < items.Count) _slots[i].SetItem(items[i], i);
            else                 _slots[i].ClearSlot();
        }
    }
}
