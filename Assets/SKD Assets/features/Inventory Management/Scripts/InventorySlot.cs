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
    public Image           itemIcon;
    public TextMeshProUGUI quantityText;

    [Header("Colors")]
    public Color normalColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private InventoryItem _item;
    private int           _slotIndex;

    // Static drag state
    public static InventorySlot DragSource      = null;
    public static ItemData      DragSourceItem  = null;
    public static int           DragSourceIndex = -1;

    void Start()
    {
        if (itemIcon == null)    Debug.LogError($"[InventorySlot] '{gameObject.name}' itemIcon NULL!");
        if (quantityText == null) Debug.LogWarning($"[InventorySlot] '{gameObject.name}' quantityText NULL!");
    }

    public void SetItem(InventoryItem item, int index)
    {
        if (item == null) { ClearSlot(); return; }
        if (item.data == null) { Debug.LogError($"[InventorySlot] Slot {index} NULL data!"); return; }

        _item      = item;
        _slotIndex = index;

        itemIcon.sprite   = _item.data.icon;
        itemIcon.color    = Color.white;
        quantityText.text = _item.quantity > 1 ? _item.quantity.ToString() : "";
    }

    public void ClearSlot()
    {
        _item = null;
        if (itemIcon != null)    { itemIcon.sprite = null; itemIcon.color = Color.clear; }
        if (quantityText != null)  quantityText.text = "";
        GetComponent<Image>().color = normalColor;
    }

    // ── Right click — drop item in world ───────────────────────────────────
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_item == null) return;
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"[InventorySlot] Right-click drop: {_item.data.itemName}");
            ItemDropper.Instance?.DropItem(_item.data);
        }
    }

    // ── Tooltip + G key hover ──────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item != null && ItemDropper.Instance != null)
        {
            ItemDropper.Instance.hoveredItem         = _item;
            ItemDropper.Instance.hoveredHotbarIndex  = -1;
        }
        if (_item == null) return;
        ItemTooltip.Instance?.Show(_item.data, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemDropper.Instance != null) ItemDropper.Instance.hoveredItem = null;
        ItemTooltip.Instance?.Hide();
    }

    // ── Drag ───────────────────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_item == null) return;

        DragSource      = this;
        DragSourceItem  = _item.data;
        DragSourceIndex = _slotIndex;

        Debug.Log($"[InventorySlot] Drag BEGIN slot {_slotIndex}: {_item.data.itemName}");
        DragIcon.Instance?.Show(_item.data.icon);
        itemIcon.color = new Color(1, 1, 1, 0.4f);
        ItemTooltip.Instance?.Hide();
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragIcon.Instance?.Follow(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragIcon.Instance?.Hide();
        if (_item != null) itemIcon.color = Color.white;

        // Only world-drop if DragSourceItem wasn't consumed by a hotbar slot
        if (eventData.pointerEnter == null && DragSourceItem != null)
        {
            Debug.Log("[InventorySlot] Dropped outside UI — world drop.");
            ItemDropper.Instance?.DropItem(DragSourceItem);
        }

        DragSource      = null;
        DragSourceItem  = null;
        DragSourceIndex = -1;
    }

    // ── Drop — accept from hotbar back into inventory ──────────────────────
    public void OnDrop(PointerEventData eventData)
    {
        // From another inventory slot — swap
        if (InventorySlot.DragSource != null && InventorySlot.DragSource != this)
        {
            Debug.Log($"[InventorySlot] Swap inv {DragSourceIndex} <-> {_slotIndex}");
            InventoryManager.Instance.SwapSlots(DragSourceIndex, _slotIndex);
            return;
        }

        // From a hotbar slot — return to inventory at this position
        if (HotbarSlotUI.DragSource != null)
        {
            int hotbarIdx = HotbarSlotUI.DragSourceIndex;
            InventoryItem hotbarItem = HotbarManager.Instance?.GetSlot(hotbarIdx);
            if (hotbarItem == null) return;

            // Return to inventory
            bool returned = HotbarManager.Instance.ReturnToInventory(hotbarIdx);
            if (returned)
            {
                // Move it to the specific slot position dropped on
                int newPos = InventoryManager.Instance.items.Count - 1;
                if (_slotIndex < newPos)
                    InventoryManager.Instance.SwapSlots(newPos, _slotIndex);

                // Consume drag so OnEndDrag doesn't world drop
                HotbarSlotUI.DragSource      = null;
                HotbarSlotUI.DragSourceIndex = -1;
                Debug.Log($"[InventorySlot] Moved hotbar item back to inventory slot {_slotIndex}.");
            }
        }
    }
}
