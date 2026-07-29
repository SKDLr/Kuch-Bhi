using UnityEngine;
using TMPro;

public class ItemTooltip : MonoBehaviour
{
    public static ItemTooltip Instance { get; private set; }

    [Header("Text Fields")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI typeText;
    public TextMeshProUGUI descText;

    [Header("Offset from cursor")]
    public Vector2 offset = new Vector2(10f, -10f);

    private RectTransform _rect;
    private Canvas        _canvas;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        Debug.Log("[ItemTooltip] Awake called.");
        Instance = this;
        _rect    = GetComponent<RectTransform>();
        _canvas  = GetComponentInParent<Canvas>();

        if (_canvas == null)
            Debug.LogError("[ItemTooltip] No parent Canvas found! " +
                           "The Tooltip object must be a child of a Canvas.");

        if (nameText == null) Debug.LogError("[ItemTooltip] nameText is NULL — assign it in the Inspector.");
        if (typeText == null) Debug.LogError("[ItemTooltip] typeText is NULL — assign it in the Inspector.");
        if (descText == null) Debug.LogError("[ItemTooltip] descText is NULL — assign it in the Inspector.");

        gameObject.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Show(ItemData data, Vector3 screenPos)
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

        gameObject.SetActive(true);
        PositionNearCursor(screenPos);
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            Debug.Log("[ItemTooltip] Hiding tooltip.");
            gameObject.SetActive(false);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!gameObject.activeSelf) return;
        PositionNearCursor(Input.mousePosition);
    }

    private void PositionNearCursor(Vector3 screenPos)
    {
        if (_canvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            screenPos,
            _canvas.worldCamera,
            out localPoint
        );

        localPoint += offset;

        Vector2 canvasSize = (_canvas.transform as RectTransform).sizeDelta;
        float halfW = _rect.sizeDelta.x / 2f;
        float halfH = _rect.sizeDelta.y / 2f;

        localPoint.x = Mathf.Clamp(localPoint.x, -canvasSize.x / 2f + halfW, canvasSize.x / 2f - halfW);
        localPoint.y = Mathf.Clamp(localPoint.y, -canvasSize.y / 2f + halfH, canvasSize.y / 2f - halfH);

        _rect.localPosition = localPoint;
    }
}