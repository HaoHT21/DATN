using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public int maxSlot = 24;

    public List<ItemData> items =
        new List<ItemData>();

    public InventorySlotUI[] slots;

    private void Awake()
    {
        Instance = this;
    }

    public bool AddItem(ItemData item)
    {
        if (items.Count >= maxSlot)
        {
            Debug.Log("Inventory đầy");
            return false;
        }

        items.Add(item);

        RefreshUI();

        return true;
    }

    public int CountItem(int itemID)
    {
        int count = 0;
        foreach (ItemData item in items)
        {
            if (item != null && item.itemID == itemID)
                count++;
        }
        return count;
    }

    public bool HasEnoughItems(int itemID, int amount)
    {
        return CountItem(itemID) >= amount;
    }

    public bool TryRemoveItems(int itemID, int amount)
    {
        if (!HasEnoughItems(itemID, amount))
            return false;

        int removed = 0;
        for (int i = items.Count - 1; i >= 0 && removed < amount; i--)
        {
            if (items[i] != null && items[i].itemID == itemID)
            {
                items.RemoveAt(i);
                removed++;
            }
        }

        RefreshUI();
        return removed == amount;
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (i < items.Count)
                slots[i].SetItem(items[i]);
            else
                slots[i].SetItem(null);
        }
    }
}