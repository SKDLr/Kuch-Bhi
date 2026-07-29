using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [Header("UI References")]
    public Image itemIcon;
    public TextMeshProUGUI quantityText;

    private InventoryItem _item;
    private int _slotIndex;
    private static InventorySlot _draggedFromSlot;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        // Warn early if Inspector refs are missing
        if (itemIcon == null)
            Debug.LogError($"[InventorySlot] '{gameObject.name}' — itemIcon is NULL! " +
                           "Assign the child Image (ItemIcon) in the Slot prefab Inspector.");

        if (quantityText == null)
            Debug.LogWarning($"[InventorySlot] '{gameObject.name}' — quantityText is NULL! " +
                             "Assign the child TMP (QuantityText) in the Slot prefab Inspector.");
    }

    // ──────────────────────────────────────────────────────────────────────
    public void SetItem(InventoryItem item, int index)
    {
        if (item == null)
        {
            Debug.LogWarning($"[InventorySlot] SetItem called with NULL item on slot {index}. Clearing.");
            ClearSlot();
            return;
        }

        if (item.data == null)
        {
            Debug.LogError($"[InventorySlot] Item at slot {index} has NULL ItemData! " +
                           "The ScriptableObject reference may be broken.");
            return;
        }

        _item = item;
        _slotIndex = index;

        itemIcon.sprite = _item.data.icon;
        itemIcon.color  = Color.white;
        quantityText.text = _item.quantity > 1 ? _item.quantity.ToString() : "";

        if (_item.data.icon == null)
            Debug.LogWarning($"[InventorySlot] Item '{_item.data.itemName}' has no icon sprite assigned. " +
                             "The slot will appear blank.");
    }

    // ──────────────────────────────────────────────────────────────────────
    public void ClearSlot()
    {
        _item = null;
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.color  = Color.clear;
        }
        if (quantityText != null)
            quantityText.text = "";
    }

    // ── Click ──────────────────────────────────────────────────────────────
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_item == null) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"[InventorySlot] Right-clicked slot {_slotIndex} — removing one {_item.data.itemName}.");
            InventoryManager.Instance.RemoveItem(_item.data);
        }
    }

    // ── Tooltip ────────────────────────────────────────────────────────────
    
    
    
    public void OnPointerEnter(PointerEventData eventData)
{
    Debug.Log("[InventorySlot] OnPointerEnter fired. Item: " + (_item?.data?.itemName ?? "NULL"));

    if (_item == null)
    {
        Debug.Log("[InventorySlot] Slot is empty — no tooltip.");
        return;
    }

    if (ItemTooltip.Instance == null)
    {
        Debug.LogError("[InventorySlot] ItemTooltip.Instance is NULL — Tooltip object is disabled or missing.");
        return;
    }

    ItemTooltip.Instance.Show(_item.data, transform.position);
}
    
    // public void OnPointerEnter(PointerEventData eventData)
    // {
    //     if (_item == null) return;

    //     if (ItemTooltip.Instance == null)
    //     {
    //         Debug.LogWarning("[InventorySlot] ItemTooltip.Instance is NULL! " +
    //                          "Make sure the Tooltip object has ItemTooltip.cs and is in the scene.");
    //         return;
    //     }

    //     ItemTooltip.Instance.Show(_item.data, transform.position);
    // }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltip.Instance?.Hide();
    }

    // ── Drag ───────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null)
        {
            Debug.Log($"[InventorySlot] Drag started on empty slot {_slotIndex} — ignoring.");
            return;
        }

        _draggedFromSlot = this;
        Debug.Log($"[InventorySlot] Drag BEGIN — slot {_slotIndex} ({_item.data.itemName})");

        if (DragIcon.Instance == null)
            Debug.LogWarning("[InventorySlot] DragIcon.Instance is NULL! " +
                             "Make sure the DragIcon object has DragIcon.cs and is in the Canvas.");
        else
            DragIcon.Instance.Show(_item.data.icon);

        itemIcon.color = new Color(1, 1, 1, 0.4f);
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragIcon.Instance?.Follow(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"[InventorySlot] Drag END — slot {_slotIndex}");
        DragIcon.Instance?.Hide();

        if (_item != null)
            itemIcon.color = Color.white;

        _draggedFromSlot = null;
    }

    // ── Drop ───────────────────────────────────────────────────────────────
    public void OnDrop(PointerEventData eventData)
    {
        if (_draggedFromSlot == null)
        {
            Debug.LogWarning("[InventorySlot] OnDrop — no drag source found. Was OnBeginDrag missed?");
            return;
        }

        if (_draggedFromSlot == this)
        {
            Debug.Log("[InventorySlot] Dropped onto the same slot — no swap needed.");
            return;
        }

        Debug.Log($"[InventorySlot] DROP — swapping slot {_draggedFromSlot._slotIndex} " +
                  $"with slot {_slotIndex}");

        InventoryManager.Instance.SwapSlots(_draggedFromSlot._slotIndex, _slotIndex);
    }
}