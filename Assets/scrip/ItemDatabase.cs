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

    private void OnValidate()
    {
        // Tự động nạp lại Database khi thay đổi danh sách trong Unity Editor
        InitializeDatabase();
    }

    /// <summary>
    /// Nạp dữ liệu từ List allItems vào Dictionary để truy xuất nhanh O(1)
    /// </summary>
    public void InitializeDatabase()
    {
        if (itemDict == null)
        {
            itemDict = new Dictionary<int, ItemData>();
        }

        itemDict.Clear();

        if (allItems == null) return;

        foreach (var item in allItems)
        {
            if (item != null)
            {
                if (!itemDict.ContainsKey(item.itemID))
                {
                    itemDict.Add(item.itemID, item);
                }
                else
                {
                    Debug.LogWarning($"[ItemDatabase] Phát hiện trùng lặp ID {item.itemID} trên item '{item.name}'!");
                }
            }
        }
    }

    /// <summary>
    /// Tìm ItemData bằng ID (Có cơ chế Fallback tìm trực tiếp trong List nếu Dictionary thiếu)
    /// </summary>
    public ItemData GetItemByID(int id)
    {
        // 1. Tìm nhanh trong Dictionary
        if (itemDict.TryGetValue(id, out var item) && item != null)
        {
            return item;
        }

        // 2. Fallback: Nếu không tìm thấy trong Dictionary, duyệt trực tiếp List allItems
        foreach (var checkItem in allItems)
        {
            if (checkItem != null && checkItem.itemID == id)
            {
                // Cập nhật ngược lại vào Dictionary để lần sau truy xuất nhanh
                itemDict[id] = checkItem;
                return checkItem;
            }
        }

        Debug.LogWarning($"[ItemDatabase] Không tìm thấy ItemData có ID: {id}. Hãy kiểm tra lại danh sách allItems trong Inspector!");
        return null;
    }
}