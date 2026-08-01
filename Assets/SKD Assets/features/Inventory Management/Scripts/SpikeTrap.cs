using UnityEngine;

// Attach to your spike trap object
// Requires a Collider with Is Trigger ON

public class SpikeTrap : MonoBehaviour
{
    [Header("Damage Settings")]
    public float damagePerSecond = 20f;  // 20 DPS = dead in 5 seconds
    public float damageInterval  = 1f;   // tick every 1 second

    [Header("Visual Feedback")]
    public bool  pulseOnDamage = true;

    private float _tickTimer    = 0f;
    private bool  _playerInside = false;
    private PlayerHealth _playerHealth;

   



    void Start()
    {
        // Validate collider setup
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError($"[SpikeTrap] '{gameObject.name}' has no Collider! Add one and enable Is Trigger.");
            return;
        }
        if (!col.isTrigger)
        {
        Debug.LogWarning($"[SpikeTrap] '{gameObject.name}' collider is NOT a trigger — enabling it now.");
        col.isTrigger = true;
        }

        // Subscribe to player events
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnDeath   += ResetTrap;
            PlayerHealth.Instance.OnRespawn += ResetTrap;
            Debug.Log($"[SpikeTrap] '{gameObject.name}' subscribed to PlayerHealth events.");
        }
        else
        {
            Debug.LogError($"[SpikeTrap] PlayerHealth.Instance is NULL in Start! " +
                       "Make sure PlayerHealth script is in the scene and runs before SpikeTrap.");
        }

        Debug.Log($"[SpikeTrap] '{gameObject.name}' ready. {damagePerSecond} DPS, ticks every {damageInterval}s.");
    }

    void OnDestroy()
    {
        // Clean up subscriptions when trap is destroyed
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnDeath   -= ResetTrap;
            PlayerHealth.Instance.OnRespawn -= ResetTrap;
        }
    }

    void ResetTrap()
    {
        _playerInside = false;
        _tickTimer    = 0f;
        _playerHealth = null;
        Debug.Log($"[SpikeTrap] '{gameObject.name}' reset.");
    }






    // // ──────────────────────────────────────────────────────────────────────
    // void Start()
    // {
    //     // Validate collider setup
    //     Collider col = GetComponent<Collider>();
    //     if (col == null)
    //     {
    //         Debug.LogError($"[SpikeTrap] '{gameObject.name}' has no Collider! Add one and enable Is Trigger.");
    //         return;
    //     }
    //     if (!col.isTrigger)
    //     {
    //         Debug.LogWarning($"[SpikeTrap] '{gameObject.name}' collider is NOT a trigger — " +
    //                          "enabling it now. Set Is Trigger ON in Inspector.");
    //         col.isTrigger = true;
    //     }

    //     Debug.Log($"[SpikeTrap] '{gameObject.name}' ready. {damagePerSecond} DPS, ticks every {damageInterval}s.");
    // }

    // ──────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (!_playerInside || _playerHealth == null) return;

        _tickTimer += Time.deltaTime;
        if (_tickTimer >= damageInterval)
        {
            _tickTimer = 0f;
            float damage = damagePerSecond * damageInterval;
            Debug.Log($"[SpikeTrap] Dealing {damage} damage to player.");
            _playerHealth.TakeDamage(damage);
        }
    }

    // ──────────────────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerHealth = other.GetComponent<PlayerHealth>();
        if (_playerHealth == null)
        {
            Debug.LogWarning("[SpikeTrap] Player entered but no PlayerHealth found! " +
                             "Make sure PlayerHealth.cs is on the Player.");
            return;
        }

        _playerInside = true;
        _tickTimer    = damageInterval; // deal first tick immediately on enter

        Debug.Log($"[SpikeTrap] Player entered '{gameObject.name}'. Starting damage ticks.");
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        _playerInside = false;
        _tickTimer    = 0f;
        _playerHealth = null;

        Debug.Log($"[SpikeTrap] Player left '{gameObject.name}'. Damage stopped.");
    }

    // ── Draw trigger area in Scene view ───────────────────────────────────
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        Collider col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawCube(transform.position, transform.localScale);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
        Gizmos.DrawWireCube(transform.position, transform.localScale);
    }


void OnEnable()
{
    if (PlayerHealth.Instance != null)
    {
        PlayerHealth.Instance.OnDeath   += ResetTrap;
        PlayerHealth.Instance.OnRespawn += ResetTrap; // ← add this
    }
}

void OnDisable()
{
    if (PlayerHealth.Instance != null)
    {
        PlayerHealth.Instance.OnDeath   -= ResetTrap;
        PlayerHealth.Instance.OnRespawn -= ResetTrap; // ← add this
    }
}





}
