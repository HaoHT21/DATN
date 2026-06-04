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