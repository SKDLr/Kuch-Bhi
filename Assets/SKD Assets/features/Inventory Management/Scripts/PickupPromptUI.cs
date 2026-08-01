using UnityEngine;
using TMPro;

// Attach to a UI Text object that sits on the Canvas
// Shows "Press E to pick up" or custom text when near an item

public class PickupPromptUI : MonoBehaviour
{
    public static PickupPromptUI Instance { get; private set; }

    [Header("References")]
    public TextMeshProUGUI promptText;

    void Awake()
    {
        Instance = this;

        if (promptText == null)
            Debug.LogError("[PickupPromptUI] promptText is NULL! Assign the TMP text in the Inspector.");

        gameObject.SetActive(false);
    }

    public void Show(string message)
    {
        if (promptText != null)
            promptText.text = message;

        gameObject.SetActive(true);
        Debug.Log("[PickupPromptUI] Showing: " + message);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        Debug.Log("[PickupPromptUI] Hidden.");
    }
}
