using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    public static ItemDropper Instance { get; private set; }

    [Header("Drop Settings")]
    public KeyCode dropKey    = KeyCode.G;
    public float dropDistance = 1.0f;
    public float dropHeight   = 1.5f;
    public float dropForce    = 1.5f;

    // Set by InventorySlot hover
    [HideInInspector] public InventoryItem hoveredItem        = null;
    // Set by HotbarSlotUI hover (-1 means none)
    [HideInInspector] public int           hoveredHotbarIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Update()
    {
        if (!Input.GetKeyDown(dropKey)) return;

        // G key on hotbar slot
        if (hoveredHotbarIndex >= 0)
        {
            Debug.Log($"[ItemDropper] G key — dropping from hotbar slot {hoveredHotbarIndex}.");
            HotbarManager.Instance?.DropFromHotbar(hoveredHotbarIndex);
            return;
        }

        // G key on inventory slot
        if (hoveredItem != null)
        {
            Debug.Log($"[ItemDropper] G key — dropping {hoveredItem.data.itemName} from inventory.");
            DropItem(hoveredItem.data);
        }
    }

    // Called when dropping from inventory bag
    public void DropItem(ItemData data)
    {
        if (data == null) { Debug.LogError("[ItemDropper] DropItem NULL!"); return; }
        InventoryManager.Instance.RemoveItem(data);
        SpawnWorldItem(data);
    }

    // Called directly with data (from hotbar drop)
    public void DropItemDirect(ItemData data)
    {
        if (data == null) return;
        SpawnWorldItem(data);
    }

    void SpawnWorldItem(ItemData data)
    {
        if (data.worldPrefab == null)
        {
            Debug.LogWarning($"[ItemDropper] {data.itemName} has no World Prefab.");
            return;
        }

        Vector3 spawnPos;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 10f))
            spawnPos = hit.point + Vector3.up * 0.5f + transform.forward * dropDistance;
        else
            spawnPos = transform.position + transform.forward * dropDistance + Vector3.up * dropHeight;

        GameObject dropped = Instantiate(data.worldPrefab, spawnPos, Quaternion.identity);
        dropped.name = data.itemName + "_dropped";

        Rigidbody rb = dropped.GetComponent<Rigidbody>();
        if (rb == null) rb = dropped.AddComponent<Rigidbody>();
        rb.isKinematic            = false;
        rb.useGravity             = true;
        rb.interpolation          = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.linearDamping          = 0.5f;
        rb.angularDamping         = 0.8f;
        rb.AddForce(transform.forward * dropForce + Vector3.up * 0.5f, ForceMode.Impulse);

        Collider rootCol = dropped.GetComponent<Collider>();
        if (rootCol == null) { var sc = dropped.AddComponent<SphereCollider>(); sc.radius = 0.25f; rootCol = sc; }
        rootCol.isTrigger = false;

        Transform old = dropped.transform.Find("PickupTrigger");
        if (old != null) Destroy(old.gameObject);

        ItemPickup pickup = dropped.GetComponent<ItemPickup>();
        if (pickup == null) pickup = dropped.AddComponent<ItemPickup>();
        pickup.itemData = data;
        pickup.enabled  = true;

        Debug.Log($"[ItemDropper] Spawned {data.itemName} at {spawnPos}.");
    }
}
