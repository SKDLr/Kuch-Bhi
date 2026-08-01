using UnityEngine;

// Attach to the HotbarPanel object
public class HotbarUI : MonoBehaviour
{
    [Header("Assign in Inspector")]
    public GameObject      hotbarSlotPrefab;  // your HotbarSlot prefab
    public Transform       slotsParent;       // the HotbarPanel itself

    private HotbarSlotUI[] _slots = new HotbarSlotUI[HotbarManager.SLOT_COUNT];

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (hotbarSlotPrefab == null)
        {
            Debug.LogError("[HotbarUI] hotbarSlotPrefab is NULL! Assign it in Inspector.");
            return;
        }
        if (slotsParent == null)
        {
            Debug.LogError("[HotbarUI] slotsParent is NULL! Drag HotbarPanel into the field.");
            return;
        }
        if (HotbarManager.Instance == null)
        {
            Debug.LogError("[HotbarUI] HotbarManager.Instance is NULL! Add HotbarManager to GameManager.");
            return;
        }

        BuildSlots();

        HotbarManager.Instance.OnHotbarChanged    += RefreshAll;
        HotbarManager.Instance.OnSelectionChanged += OnSelectionChanged;

        // Highlight slot 0 on start
        OnSelectionChanged(0);

        Debug.Log("[HotbarUI] Ready.");
    }

    void OnDestroy()
    {
        if (HotbarManager.Instance == null) return;
        HotbarManager.Instance.OnHotbarChanged    -= RefreshAll;
        HotbarManager.Instance.OnSelectionChanged -= OnSelectionChanged;
    }

    // ──────────────────────────────────────────────────────────────────────
    void BuildSlots()
    {
        // Clear any existing children first
        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);

        for (int i = 0; i < HotbarManager.SLOT_COUNT; i++)
        {
            GameObject go  = Instantiate(hotbarSlotPrefab, slotsParent);
            go.name        = $"HotbarSlot_{i}";

            HotbarSlotUI s = go.GetComponent<HotbarSlotUI>();
            if (s == null)
            {
                Debug.LogError($"[HotbarUI] HotbarSlot_{i} prefab is missing HotbarSlotUI component!");
                continue;
            }

            s.Init(i);
            _slots[i] = s;
        }

        Debug.Log($"[HotbarUI] Built {HotbarManager.SLOT_COUNT} slots.");
    }

    // ──────────────────────────────────────────────────────────────────────
    void RefreshAll()
    {
        foreach (var slot in _slots)
            slot?.Refresh();
    }

    void OnSelectionChanged(int selected)
    {
        for (int i = 0; i < _slots.Length; i++)
            _slots[i]?.SetSelected(i == selected);
    }
}
