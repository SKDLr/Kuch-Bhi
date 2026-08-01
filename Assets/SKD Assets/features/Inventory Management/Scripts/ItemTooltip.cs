using UnityEngine;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Header("Text Fields")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI descText;

    [Header("Offset — gap between slot left edge and tooltip right edge")]
    public float sideOffsetX = 10f;

    private RectTransform _rect;
    private Canvas        _canvas;
    private RectTransform _anchorSlot;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        Debug.Log("[ItemTooltip] Awake called.");
        Instance = this;
        _rect    = GetComponent<RectTransform>();
        _canvas  = GetComponentInParent<Canvas>();

        if (_canvas == null)
            Debug.LogError("[ItemTooltip] No parent Canvas found! The Tooltip must be a child of the Canvas.");
        if (nameText == null) Debug.LogError("[ItemTooltip] nameText is NULL — assign it in the Inspector.");
        if (typeText == null) Debug.LogError("[ItemTooltip] typeText is NULL — assign it in the Inspector.");
        if (descText == null) Debug.LogError("[ItemTooltip] descText is NULL — assign it in the Inspector.");

        gameObject.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Show(ItemData data, RectTransform slotRect)
    {
        if (data == null)
        {
            Debug.LogError("[ItemTooltip] Show called with NULL ItemData!");
            return;
        }

        Debug.Log($"[ItemTooltip] Showing tooltip for: {data.itemName}");

        nameText.text = data.itemName;
        typeText.text = data.type.ToString();
        descText.text = data.description;

        _anchorSlot = slotRect;
        gameObject.SetActive(true);

        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(_rect);
        PositionToLeftOfSlot();
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            Debug.Log("[ItemTooltip] Hiding tooltip.");
            _anchorSlot = null;
            gameObject.SetActive(false);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!gameObject.activeSelf || _anchorSlot == null) return;
        PositionToLeftOfSlot();
    }

    // ──────────────────────────────────────────────────────────────────────
    private void PositionToLeftOfSlot()
    {
        if (_canvas == null || _anchorSlot == null) return;

        // Get the four world-space corners of the slot
        // Order: 0=BottomLeft  1=TopLeft  2=TopRight  3=BottomRight
        Vector3[] corners = new Vector3[4];
        _anchorSlot.GetWorldCorners(corners);

        // Left-center of the slot in world space
        Vector3 slotLeftCenter = (corners[0] + corners[1]) / 2f;

        // Convert to screen space then to canvas local space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, slotLeftCenter);
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            screenPoint,
            _canvas.worldCamera,
            out localPoint
        );

        // Shift LEFT so tooltip's right edge sits beside the slot's left edge
        float tooltipHalfW = _rect.sizeDelta.x / 2f;
        float tooltipHalfH = _rect.sizeDelta.y / 2f;
        localPoint.x -= tooltipHalfW + sideOffsetX;

        // Clamp so tooltip never goes off screen
        Vector2 canvasSize = (_canvas.transform as RectTransform).sizeDelta;
        localPoint.x = Mathf.Clamp(localPoint.x, -canvasSize.x / 2f + tooltipHalfW, canvasSize.x / 2f - tooltipHalfW);
        localPoint.y = Mathf.Clamp(localPoint.y, -canvasSize.y / 2f + tooltipHalfH, canvasSize.y / 2f - tooltipHalfH);

        _rect.localPosition = localPoint;
    }
}