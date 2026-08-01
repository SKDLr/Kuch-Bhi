using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Attach to the HealthBarPanel object
public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public Image           fillBar;       // the red fill image
    public Image           backgroundBar; // dark background bar
    public TextMeshProUGUI healthText;    // shows "75 / 100"

    [Header("Colors")]
    public Color highColor  = new Color(0.82f, 0.15f, 0.15f, 1f); // red
    public Color midColor   = new Color(0.9f,  0.6f,  0.1f,  1f); // orange
    public Color lowColor   = new Color(0.9f,  0.9f,  0.1f,  1f); // yellow

    [Header("Thresholds")]
    public float midThreshold = 0.5f;  // below 50% → orange
    public float lowThreshold = 0.25f; // below 25% → yellow

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (fillBar == null)
            Debug.LogError("[HealthBarUI] fillBar is NULL! Assign the red fill Image in Inspector.");
        if (healthText == null)
            Debug.LogWarning("[HealthBarUI] healthText is NULL — no text will show.");

        if (PlayerHealth.Instance == null)
        {
            Debug.LogError("[HealthBarUI] PlayerHealth.Instance is NULL! " +
                           "Make sure PlayerHealth is on the Player and loads before HealthBarUI.");
            return;
        }

        // Subscribe to health changes
        PlayerHealth.Instance.OnHealthChanged += UpdateBar;
        PlayerHealth.Instance.OnDeath         += OnPlayerDeath;
        PlayerHealth.Instance.OnRespawn       += OnPlayerRespawn;

        // Initialize to full health
        UpdateBar(PlayerHealth.Instance.currentHealth, PlayerHealth.Instance.maxHealth);
        Debug.Log("[HealthBarUI] Ready.");
    }

    void OnDestroy()
    {
        if (PlayerHealth.Instance == null) return;
        PlayerHealth.Instance.OnHealthChanged -= UpdateBar;
        PlayerHealth.Instance.OnDeath         -= OnPlayerDeath;
        PlayerHealth.Instance.OnRespawn       -= OnPlayerRespawn;
    }

    // ──────────────────────────────────────────────────────────────────────
    void UpdateBar(float current, float max)
    {
        float pct = max > 0 ? current / max : 0f;

        // Update fill amount
        if (fillBar != null)
        {
            fillBar.fillAmount = pct;

            // Color shifts based on health percentage
            if (pct > midThreshold)
                fillBar.color = highColor;
            else if (pct > lowThreshold)
                fillBar.color = midColor;
            else
                fillBar.color = lowColor;
        }

        // Update text
        if (healthText != null)
            healthText.text = $"{Mathf.CeilToInt(current)} / {Mathf.CeilToInt(max)}";

        Debug.Log($"[HealthBarUI] Updated — {current}/{max} ({pct * 100:F0}%)");
    }

    // ──────────────────────────────────────────────────────────────────────
    void OnPlayerDeath()
    {
        Debug.Log("[HealthBarUI] Player died — bar at 0.");
        if (fillBar != null) fillBar.fillAmount = 0f;
        if (healthText != null) healthText.text = "0 / 100";
    }

    void OnPlayerRespawn()
    {
        Debug.Log("[HealthBarUI] Player respawned — bar refilled.");
    }
}
