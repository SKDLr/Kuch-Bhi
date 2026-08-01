using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class HotbarSlotUI : MonoBehaviour,
    IDropHandler,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [Header("Child References")]
    public Image           iconImage;
    public Image           highlight;
    public TextMeshProUGUI numberLabel;

    [Header("Colors")]
    public Color normalBg    = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    public Color selectedBg  = new Color(0.9f,  0.75f, 0.1f,  1.0f);
    public Color highlightOn = new Color(1f,    0.9f,  0.3f,  0.5f);

    private int   _index;
    private Image _bg;

    // Static drag state for hotbar→anywhere drags
    public static HotbarSlotUI DragSource      = null;
    public static int          DragSourceIndex = -1;

    // ──────────────────────────────────────────────────────────────────────
    public void Init(int index)
    {
        _index = index;
        _bg    = GetComponent<Image>();
        if (numberLabel != null) numberLabel.text = (index + 1).ToString();
        Refresh();
        SetSelected(false);
    }

    public void Refresh()
    {
        InventoryItem item = HotbarManager.Instance?.GetSlot(_index);
        if (item != null && item.data?.icon != null)
        {
            iconImage.sprite = item.data.icon;
            iconImage.color  = Color.white;

            // Show quantity if stack is more than 1
            if (numberLabel != null && item.data.isStackable && item.quantity > 1)
                numberLabel.text = item.quantity.ToString();
            else if (numberLabel != null)
                numberLabel.text = (_index + 1).ToString(); // restore slot number
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color  = Color.clear;
            if (numberLabel != null) numberLabel.text = (_index + 1).ToString();
        }
    }

    public void SetSelected(bool selected)
    {
        if (_bg != null)        _bg.color       = selected ? selectedBg  : normalBg;
        if (highlight != null)  highlight.color = selected ? highlightOn : Color.clear;
    }

    // ── Tooltip ────────────────────────────────────────────────────────────
    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryItem item = HotbarManager.Instance?.GetSlot(_index);
        if (item == null) return;

        // Set hovered for G key drop
        if (ItemDropper.Instance != null)
            ItemDropper.Instance.hoveredHotbarIndex = _index;

        ItemTooltip.Instance?.Show(item.data, GetComponent<RectTransform>());
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (ItemDropper.Instance != null)
            ItemDropper.Instance.hoveredHotbarIndex = -1;
        ItemTooltip.Instance?.Hide();
    }

    // ── Drop FROM inventory slot or another hotbar slot ────────────────────
    public void OnDrop(PointerEventData eventData)
    {
        // Coming from an inventory slot
        if (InventorySlot.DragSource != null && InventorySlot.DragSourceItem != null)
        {
            // Find the actual InventoryItem object in inventory list
            InventoryItem invItem = InventoryManager.Instance.items
                .Find(i => i.data == InventorySlot.DragSourceItem);

            if (invItem != null)
            {
                HotbarManager.Instance?.MoveFromInventory(invItem, _index);
                InventorySlot.DragSourceItem = null; // prevent world drop
                Debug.Log($"[HotbarSlotUI] Moved {invItem.data.itemName} from inventory to hotbar {_index}.");
            }
            return;
        }

        // Coming from another hotbar slot
        if (DragSource != null && DragSource != this)
        {
            HotbarManager.Instance?.SwapHotbarSlots(DragSourceIndex, _index);
            Debug.Log($"[HotbarSlotUI] Swapped hotbar {DragSourceIndex} <-> {_index}.");
        }
    }

    // ── Drag FROM hotbar ───────────────────────────────────────────────────
    public void OnBeginDrag(PointerEventData eventData)
    {
        InventoryItem item = HotbarManager.Instance?.GetSlot(_index);
        if (item == null) return;

        DragSource      = this;
        DragSourceIndex = _index;

        Debug.Log($"[HotbarSlotUI] Drag BEGIN hotbar slot {_index}: {item.data.itemName}");
        DragIcon.Instance?.Show(item.data.icon);
        iconImage.color = new Color(1, 1, 1, 0.4f);
        ItemTooltip.Instance?.Hide();
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragIcon.Instance?.Follow(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragIcon.Instance?.Hide();
        Refresh();

        // Dropped outside all UI — drop in world
        if (eventData.pointerEnter == null && DragSourceIndex >= 0)
        {
            Debug.Log($"[HotbarSlotUI] Dragged hotbar slot {_index} outside UI — dropping in world.");
            HotbarManager.Instance?.DropFromHotbar(DragSourceIndex);
        }

        DragSource      = null;
        DragSourceIndex = -1;
    }

    // ── Right click — return item to inventory ─────────────────────────────
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;

        InventoryItem item = HotbarManager.Instance?.GetSlot(_index);
        if (item == null) return;

        bool returned = HotbarManager.Instance.ReturnToInventory(_index);
        if (!returned)
            Debug.LogWarning($"[HotbarSlotUI] Could not return {item.data.itemName} — inventory full.");
        else
            Debug.Log($"[HotbarSlotUI] Returned {item.data.itemName} to inventory.");
    }
}
