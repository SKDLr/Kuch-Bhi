using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item to give the player")]
    public ItemData itemData;

    [Header("Optional: rotate the pickup")]
    public bool spin = true;
    public float spinSpeed = 60f;

    // ──────────────────────────────────────────────────────────────────────
    void Start()
    {
        if (itemData == null)
            Debug.LogError($"[ItemPickup] '{gameObject.name}' has no ItemData assigned! " +
                           "Drag a ScriptableObject item into the Item Data field in the Inspector.");

        Collider col = GetComponent<Collider>();
        if (col == null)
            Debug.LogError($"[ItemPickup] '{gameObject.name}' has no Collider! " +
                           "Add a Collider and enable 'Is Trigger'.");
        else if (!col.isTrigger)
            Debug.LogWarning($"[ItemPickup] '{gameObject.name}' collider is NOT set to Is Trigger. " +
                             "OnTriggerEnter won't fire — enable Is Trigger on the Collider.");
    }

    // ──────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (spin)
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
    }

    // ──────────────────────────────────────────────────────────────────────
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ItemPickup] Trigger entered by: {other.gameObject.name} (tag: {other.tag})");

        if (!other.CompareTag("Player"))
        {
            Debug.Log("[ItemPickup] Not the Player — ignoring.");
            return;
        }

        if (itemData == null)
        {
            Debug.LogError($"[ItemPickup] Cannot pick up '{gameObject.name}' — ItemData is NULL!");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[ItemPickup] InventoryManager.Instance is NULL! " +
                           "Is there a GameManager with InventoryManager.cs in the scene?");
            return;
        }

        Debug.Log($"[ItemPickup] Player touched {itemData.itemName} — attempting to add to inventory.");
        bool picked = InventoryManager.Instance.AddItem(itemData);

        if (picked)
        {
            Debug.Log($"[ItemPickup] {itemData.itemName} picked up successfully. Destroying world object.");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"[ItemPickup] Could not pick up {itemData.itemName} — inventory may be full.");
        }
    }
}