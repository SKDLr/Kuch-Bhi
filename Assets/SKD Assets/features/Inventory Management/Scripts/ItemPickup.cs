using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;

    [Header("Detection")]
    public float pickupRange = 2.0f;  // how close player needs to be

    private bool      _playerInRange = false;
    private Transform _playerTransform;
    private Rigidbody _rb;

    // ──────────────────────────────────────────────────────────────────────
    void Awake()
    {
        _rb             = GetComponent<Rigidbody>();
        _rb.useGravity  = true;
        _rb.isKinematic = false;
    }

    void Start()
    {
        if (itemData == null)
            Debug.LogError($"[ItemPickup] '{gameObject.name}' has no ItemData!");

        // Make sure root collider is solid so item lands on ground
        Collider col = GetComponent<Collider>();
        if (col != null && col.isTrigger)
        {
            col.isTrigger = false;
            Debug.LogWarning($"[ItemPickup] Fixed root collider to non-trigger on {gameObject.name}");
        }

        // Find the player by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            _playerTransform = player.transform;
        else
            Debug.LogWarning("[ItemPickup] Could not find Player tag — pickup range won't work. " +
                             "Make sure your Player has the 'Player' tag.");

        Debug.Log($"[ItemPickup] Ready: {itemData?.itemName}, range: {pickupRange}");
    }

    // ──────────────────────────────────────────────────────────────────────
    // Distance based — works with CharacterController, Rigidbody, anything
    void Update()
    {
        if (_playerTransform == null) return;

        float dist = Vector3.Distance(transform.position, _playerTransform.position);
        bool inRange = dist <= pickupRange;

        // Player just entered range
        if (inRange && !_playerInRange)
        {
            _playerInRange = true;
            string prompt  = itemData != null ? itemData.pickupPrompt : "Press E to pick up";
            PickupPromptUI.Instance?.Show(prompt);
            Debug.Log($"[ItemPickup] Player in range of {itemData?.itemName} (dist: {dist:F2})");
        }

        // Player just left range
        if (!inRange && _playerInRange)
        {
            _playerInRange = false;
            PickupPromptUI.Instance?.Hide();
            Debug.Log($"[ItemPickup] Player left range of {itemData?.itemName}");
        }

        // E key pressed while in range
        if (_playerInRange && Input.GetKeyDown(KeyCode.E))
            TryPickup();
    }

    // ──────────────────────────────────────────────────────────────────────
    void TryPickup()
    {
        if (itemData == null)
        {
            Debug.LogError("[ItemPickup] Cannot pick up — itemData is NULL!");
            return;
        }

        bool picked = InventoryManager.Instance.AddItem(itemData);

        if (picked)
        {
            Debug.Log($"[ItemPickup] {itemData.itemName} picked up.");
            PickupPromptUI.Instance?.Hide();
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"[ItemPickup] Inventory full — could not pick up {itemData.itemName}.");
            PickupPromptUI.Instance?.Show("Inventory full!");
        }
    }

    // ── Draw range in Scene view so you can see it ─────────────────────────
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}











// using UnityEngine;

// // Attach to the ROOT of any world item prefab
// // The root needs: Rigidbody + a solid Collider (isTrigger OFF)
// // A child called "PickupTrigger" with a trigger Collider is added automatically by ItemDropper
// // OR you can add it manually in your prefab

// [RequireComponent(typeof(Rigidbody))]
// public class ItemPickup : MonoBehaviour
// {
//     [Header("Item Data")]
//     public ItemData itemData;

//     private bool _playerInRange = false;
//     private Rigidbody _rb;

//     // ──────────────────────────────────────────────────────────────────────
//     void Awake()
//     {
//         _rb             = GetComponent<Rigidbody>();
//         _rb.useGravity  = true;
//         _rb.isKinematic = false;
//         Debug.Log($"[ItemPickup] Awake on {gameObject.name}");
//     }

//     void Start()
//     {
//         if (itemData == null)
//             Debug.LogError($"[ItemPickup] '{gameObject.name}' has no ItemData assigned!");

//         // Make sure root collider is NOT a trigger
//         Collider col = GetComponent<Collider>();
//         if (col != null && col.isTrigger)
//         {
//             col.isTrigger = false;
//             Debug.LogWarning($"[ItemPickup] Fixed root collider to non-trigger on {gameObject.name}");
//         }

//         // Find or create the pickup trigger child
//         SetupTriggerChild();
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     void SetupTriggerChild()
//     {
//         // Check if PickupTrigger child already exists
//         Transform existing = transform.Find("PickupTrigger");
//         if (existing != null)
//         {
//             // Make sure it has a PickupDetector
//             if (existing.GetComponent<PickupDetector>() == null)
//                 existing.gameObject.AddComponent<PickupDetector>().Setup(this);
//             Debug.Log($"[ItemPickup] Found existing PickupTrigger on {gameObject.name}");
//             return;
//         }

//         // Create it
//         GameObject trigger = new GameObject("PickupTrigger");
//         trigger.transform.SetParent(transform);
//         trigger.transform.localPosition = Vector3.zero;
//         trigger.layer = gameObject.layer;

//         SphereCollider sc = trigger.AddComponent<SphereCollider>();
//         sc.isTrigger = true;
//         sc.radius    = 1.8f;

//         trigger.AddComponent<PickupDetector>().Setup(this);
//         Debug.Log($"[ItemPickup] Created PickupTrigger child on {gameObject.name}");
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     // Called by PickupDetector when player enters/exits range
//     public void OnPlayerEnterRange()
//     {
//         _playerInRange = true;
//         string prompt = itemData != null ? itemData.pickupPrompt : "Press E to pick up";
//         PickupPromptUI.Instance?.Show(prompt);
//         Debug.Log($"[ItemPickup] Player in range of {itemData?.itemName}");
//     }

//     public void OnPlayerExitRange()
//     {
//         _playerInRange = false;
//         PickupPromptUI.Instance?.Hide();
//         Debug.Log($"[ItemPickup] Player left range of {itemData?.itemName}");
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     void Update()
//     {
//         if (_playerInRange && Input.GetKeyDown(KeyCode.E))
//             TryPickup();
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     void TryPickup()
//     {
//         if (itemData == null)
//         {
//             Debug.LogError("[ItemPickup] Cannot pick up — itemData is NULL!");
//             return;
//         }

//         bool picked = InventoryManager.Instance.AddItem(itemData);

//         if (picked)
//         {
//             Debug.Log($"[ItemPickup] {itemData.itemName} picked up successfully.");
//             PickupPromptUI.Instance?.Hide();
//             Destroy(gameObject);
//         }
//         else
//         {
//             Debug.LogWarning($"[ItemPickup] Inventory full — could not pick up {itemData.itemName}.");
//             PickupPromptUI.Instance?.Show("Inventory full!");
//         }
//     }
// }



// using UnityEngine;

// // Attach this to any world item prefab
// // Requires a Rigidbody and a Collider (NOT trigger) on the object

// [RequireComponent(typeof(Rigidbody))]
// public class ItemPickup : MonoBehaviour
// {
//     [Header("Item Data")]
//     public ItemData itemData;

//     // ── Internal ───────────────────────────────────────────────────────────
//     private bool _playerInRange = false;
//     private Rigidbody _rb;

//     // ──────────────────────────────────────────────────────────────────────
//     void Awake()
//     {
//         _rb = GetComponent<Rigidbody>();
//         _rb.useGravity  = true;
//         _rb.isKinematic = false;
//     }

//     void Start()
//     {
//         if (itemData == null)
//             Debug.LogError($"[ItemPickup] '{gameObject.name}' has no ItemData assigned!");

//         // Make sure collider is NOT a trigger — physics needs it solid
//         Collider col = GetComponent<Collider>();
//         if (col == null)
//         {
//             Debug.LogError($"[ItemPickup] '{gameObject.name}' has no Collider! Add one.");
//         }
//         else if (col.isTrigger)
//         {
//             Debug.LogWarning($"[ItemPickup] '{gameObject.name}' collider was a trigger — setting to solid for gravity.");
//             col.isTrigger = false;
//         }
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     void Update()
//     {
//         if (_playerInRange && Input.GetKeyDown(KeyCode.E))
//         {
//             TryPickup();
//         }
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     // Use a separate child trigger collider for detection range
//     void OnTriggerEnter(Collider other)
//     {
//         if (!other.CompareTag("Player")) return;
//         _playerInRange = true;

//         string prompt = itemData != null ? itemData.pickupPrompt : "Press E to pick up";
//         PickupPromptUI.Instance?.Show(prompt);
//         Debug.Log($"[ItemPickup] Player in range of {itemData?.itemName}. Showing prompt: {prompt}");
//     }

//     void OnTriggerExit(Collider other)
//     {
//         if (!other.CompareTag("Player")) return;
//         _playerInRange = false;
//         PickupPromptUI.Instance?.Hide();
//         Debug.Log($"[ItemPickup] Player left range of {itemData?.itemName}.");
//     }

//     // ──────────────────────────────────────────────────────────────────────
//     void TryPickup()
//     {
//         if (itemData == null)
//         {
//             Debug.LogError("[ItemPickup] Cannot pick up — ItemData is NULL!");
//             return;
//         }

//         if (InventoryManager.Instance == null)
//         {
//             Debug.LogError("[ItemPickup] InventoryManager.Instance is NULL!");
//             return;
//         }

//         bool picked = InventoryManager.Instance.AddItem(itemData);
//         if (picked)
//         {
//             Debug.Log($"[ItemPickup] {itemData.itemName} picked up.");
//             PickupPromptUI.Instance?.Hide();
//             Destroy(gameObject);
//         }
//         else
//         {
//             Debug.LogWarning($"[ItemPickup] Could not pick up {itemData.itemName} — inventory full.");
//             PickupPromptUI.Instance?.Show("Inventory full!");
//         }
//     }
// }
