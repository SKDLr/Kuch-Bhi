using UnityEngine;
using UnityEngine.UI;

public class DragIcon : MonoBehaviour
{
    public static DragIcon Instance { get; private set; }

    private Image         _image;
    private RectTransform _rect;
    private Canvas        _canvas;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        Debug.Log("[DragIcon] Awake called.");
        Instance = this;
        _image   = GetComponent<Image>();
        _rect    = GetComponent<RectTransform>();
        _canvas  = GetComponentInParent<Canvas>();

        if (_image == null)
            Debug.LogError("[DragIcon] No Image component found on DragIcon object! Add one.");

        if (_canvas == null)
            Debug.LogError("[DragIcon] No parent Canvas found! DragIcon must be a child of the Canvas.");

        // CRITICAL: Raycast must be OFF or drops on other slots will be blocked
        if (_image != null && _image.raycastTarget)
        {
            Debug.LogWarning("[DragIcon] Raycast Target is ON — turning it OFF automatically. " +
                             "Please disable it in the Inspector too so drop events reach slots.");
            _image.raycastTarget = false;
        }

        gameObject.SetActive(false);
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Show(Sprite icon)
    {
        if (icon == null)
            Debug.LogWarning("[DragIcon] Show called with NULL sprite. The drag ghost will be invisible.");

        _image.sprite = icon;
        _image.color  = new Color(1f, 1f, 1f, 0.8f);
        gameObject.SetActive(true);
        Debug.Log("[DragIcon] Ghost icon shown.");
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Follow(Vector2 screenPosition)
    {
        if (_canvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvas.transform as RectTransform,
            screenPosition,
            _canvas.worldCamera,
            out localPoint
        );
        _rect.localPosition = localPoint;
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Hide()
    {
        gameObject.SetActive(false);
        if (_image != null) _image.sprite = null;
        Debug.Log("[DragIcon] Ghost icon hidden.");
    }
}