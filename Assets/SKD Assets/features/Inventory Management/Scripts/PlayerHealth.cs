using System;
using UnityEngine;
using UnityEngine.SceneManagement;

// Attach to Player
public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Health Settings")]
    public float maxHealth    = 100f;
    public float currentHealth;

    [Header("Respawn")]
    public Transform respawnPoint; // drag an empty object here in Inspector

    [Header("Invincibility after hit (seconds)")]
    public float invincibilityDuration = 0.5f;

    private float _invincibilityTimer = 0f;
    private bool  _isDead = false;

    // UI listens to this to update the bar
    public event Action<float, float> OnHealthChanged; // current, max
    public event Action               OnDeath;
    public event Action               OnRespawn;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Debug.Log($"[PlayerHealth] Initialized. HP: {currentHealth}/{maxHealth}");

        if (respawnPoint == null)
            Debug.LogWarning("[PlayerHealth] respawnPoint is NULL! " +
                             "Create an empty GameObject at your spawn location and drag it here.");
    }

    void Update()
    {
        // Count down invincibility window
        if (_invincibilityTimer > 0f)
            _invincibilityTimer -= Time.deltaTime;
    }

    // ──────────────────────────────────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (_isDead) return;
        if (_invincibilityTimer > 0f)
        {
            Debug.Log("[PlayerHealth] Hit blocked by invincibility window.");
            return;
        }

        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        _invincibilityTimer = invincibilityDuration;

        Debug.Log($"[PlayerHealth] Took {amount} damage. HP: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0f)
            Die();
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Heal(float amount)
    {
        if (_isDead) return;
        if (amount <= 0f) return;

        float before = currentHealth;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);

        Debug.Log($"[PlayerHealth] Healed {amount}. HP: {before} → {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    // ──────────────────────────────────────────────────────────────────────
    void Die()
    {
        if (_isDead) return;
        _isDead = true;

        Debug.Log("[PlayerHealth] Player DIED.");
        OnDeath?.Invoke();



        // Freeze player movement
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.inventoryOpen = true; // reuses the freeze flag

        Invoke(nameof(Respawn), 1.5f); // small delay before respawn
    }

    // ──────────────────────────────────────────────────────────────────────
    void Respawn()
    {
        _isDead = false;
        currentHealth = maxHealth;


        // Move player to respawn point
        CharacterController cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false; // must disable before teleport
        
        

        if (respawnPoint != null)
            transform.position = respawnPoint.position;
        if (cc != null) cc.enabled = true;

        // Unfreeze movement
        PlayerController pc = GetComponent<PlayerController>();
        if (pc != null) pc.inventoryOpen = false;

        _invincibilityTimer = 2f; // 2 seconds invincibility after respawn

        Debug.Log($"[PlayerHealth] Respawned at {respawnPoint?.position}. HP: {currentHealth}/{maxHealth}");
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        OnRespawn?.Invoke();
    }

    // ── Context menu helpers ───────────────────────────────────────────────
    [ContextMenu("Take 10 Damage (Test)")]
    void TestDamage() => TakeDamage(10f);

    [ContextMenu("Heal 10 (Test)")]
    void TestHeal() => Heal(10f);

    [ContextMenu("Kill Player (Test)")]
    void TestDeath() => TakeDamage(9999f);
}
