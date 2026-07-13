using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public sealed class InventoryManagerSaveable : MonoBehaviour, ISaveable
{
    [Serializable]
    private sealed class State
    {
        public List<int> itemIds = new List<int>();
    }

    public object CaptureState()
    {
        if (InventoryManager.Instance == null || InventoryManager.Instance.items == null)
            return null;

        var state = new State();
        foreach (ItemData item in InventoryManager.Instance.items)
        {
            if (item == null)
                continue;

            state.itemIds.Add(item.itemID);
        }

        return state;
    }

    public void RestoreState(object state)
    {
        if (state is not State s)
            return;

        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.items.Clear();

        for (int i = 0; i < s.itemIds.Count; i++)
        {
            int id = s.itemIds[i];
            ItemData data = FindItemDataById(id);
            if (data != null)
                InventoryManager.Instance.items.Add(data);
        }

        InventoryManager.Instance.RefreshUI();
    }

    private static ItemData FindItemDataById(int id)
    {
        if (ShopManager.Instance != null)
        {
            // ShopManager chỉ trả prefab, nên mình sẽ dò qua danh sách itemData của shop bằng reflection nhẹ tránh sửa nhiều file.
            // Nếu không tìm được thì fallback dưới.
            try
            {
                var field = typeof(ShopManager).GetField("allShopItems",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    var list = field.GetValue(ShopManager.Instance) as List<ItemData>;
                    if (list != null)
                        return list.Find(x => x != null && x.itemID == id);
                }
            }
            catch
            {
                // ignore
            }
        }

        ItemData[] all = Resources.FindObjectsOfTypeAll<ItemData>();
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].itemID == id)
                return all[i];
        }

        return null;
    }
}

