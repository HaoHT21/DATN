using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public sealed class InventoryDataSaveable : MonoBehaviour, ISaveable
{
    [Serializable]
    private sealed class State
    {
        public int currentWeaponIndex;
        public List<int> itemIds = new List<int>();
    }

    public object CaptureState()
    {
        if (InventoryData.Instance == null || InventoryData.Instance.sharedInventory == null)
            return null;

        var state = new State();
        state.currentWeaponIndex = InventoryData.Instance.currentWeaponIndex;

        foreach (var item in InventoryData.Instance.sharedInventory)
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

        if (InventoryData.Instance == null)
            return;

        InventoryData.Instance.sharedInventory.Clear();
        InventoryData.Instance.currentWeaponIndex = Mathf.Max(0, s.currentWeaponIndex);

        // Khôi phục tối thiểu theo itemID. Nếu chưa có database để rebuild icon/prefab,
        // ta tạo placeholder để tránh null list và cho phép gameplay tiếp tục.
        for (int i = 0; i < s.itemIds.Count; i++)
        {
            int id = s.itemIds[i];
            InventoryData.Instance.sharedInventory.Add(new PlayerController.WeaponItem
            {
                itemID = id,
                icon = null,
                visualPrefab = null,
                pickupPrefab = null,
                isGun = false,
                damage = 0,
                bulletPrefab = null,
                isPotion = false,
                healAmount = 0
            });
        }
    }
}

