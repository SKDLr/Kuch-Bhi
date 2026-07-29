using System.Collections.Generic;
using UnityEngine;

public class InventorySaveLoad : MonoBehaviour
{
    private const string SAVE_KEY = "InventorySave";

    [System.Serializable]
    private class SlotSaveData
    {
        public string itemName;
        public int    quantity;
    }

    [System.Serializable]
    private class InventorySaveData
    {
        public List<SlotSaveData> slots = new List<SlotSaveData>();
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Save()
    {
        if (InventoryManager.Instance == null)
        {
            Debug.LogError("[InventorySaveLoad] Save failed — InventoryManager.Instance is NULL!");
            return;
        }

        InventorySaveData saveData = new InventorySaveData();

        foreach (InventoryItem item in InventoryManager.Instance.items)
        {
            if (item.data == null)
            {
                Debug.LogWarning("[InventorySaveLoad] Skipping item with NULL data during save.");
                continue;
            }

            saveData.slots.Add(new SlotSaveData
            {
                itemName = item.data.name,
                quantity = item.quantity
            });
        }

        string json = JsonUtility.ToJson(saveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();

        Debug.Log($"[InventorySaveLoad] Saved {saveData.slots.Count} item(s) to PlayerPrefs. JSON: {json}");
    }

    // ──────────────────────────────────────────────────────────────────────
    public void Load()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY))
        {
            Debug.Log("[InventorySaveLoad] No save data found in PlayerPrefs (key: " + SAVE_KEY + ").");
            return;
        }

        string json = PlayerPrefs.GetString(SAVE_KEY);
        Debug.Log("[InventorySaveLoad] Loading save data: " + json);

        InventorySaveData saveData;
        try
        {
            saveData = JsonUtility.FromJson<InventorySaveData>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[InventorySaveLoad] Failed to parse save JSON! Data may be corrupted. Error: " + e.Message);
            return;
        }

        InventoryManager.Instance.items.Clear();
        int loaded = 0;

        foreach (SlotSaveData slot in saveData.slots)
        {
            ItemData data = Resources.Load<ItemData>("Items/" + slot.itemName);

            if (data == null)
            {
                Debug.LogWarning($"[InventorySaveLoad] Could not load item '{slot.itemName}' from Resources/Items/. " +
                                 "Make sure the ScriptableObject is inside Assets/Resources/Items/ folder.");
                continue;
            }

            InventoryItem restored = new InventoryItem(data);
            restored.quantity = slot.quantity;
            InventoryManager.Instance.items.Add(restored);
            loaded++;
            Debug.Log($"[InventorySaveLoad] Restored: {data.itemName} x{slot.quantity}");
        }

        Debug.Log($"[InventorySaveLoad] Load complete. {loaded}/{saveData.slots.Count} items restored.");
    }

    // ──────────────────────────────────────────────────────────────────────
    void OnApplicationQuit()
    {
        Debug.Log("[InventorySaveLoad] Application quitting — auto-saving inventory.");
        Save();
    }

    // ── Manual clear (useful during testing) ───────────────────────────────
    [ContextMenu("Clear Save Data")]
    public void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(SAVE_KEY);
        Debug.Log("[InventorySaveLoad] Save data cleared from PlayerPrefs.");
    }
}