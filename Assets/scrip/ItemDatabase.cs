using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public static ItemDatabase Instance { get; private set; }

    [Header("Danh sách tất cả các Item trong Game")]
    public List<ItemData> allItems = new List<ItemData>();

    private Dictionary<int, ItemData> itemDict = new Dictionary<int, ItemData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        itemDict.Clear();
        foreach (var item in allItems)
        {
            if (item != null && !itemDict.ContainsKey(item.itemID))
            {
                itemDict.Add(item.itemID, item);
            }
        }
    }

    /// <summary>
    /// Tìm ItemData bằng ID
    /// </summary>
    public ItemData GetItemByID(int id)
    {
        if (itemDict.TryGetValue(id, out var item))
        {
            return item;
        }

        Debug.LogWarning($"[ItemDatabase] Không tìm thấy ItemData có ID: {id}");
        return null;
    }
}