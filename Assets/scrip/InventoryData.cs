using UnityEngine;
using System.Collections.Generic;

public class InventoryData : MonoBehaviour
{
    public const string StableSaveId = "inventory-data";

    public static InventoryData Instance;

    // Lưu danh sách vũ khí dùng chung cho mọi nhân vật
    public List<PlayerController.WeaponItem> sharedInventory = new List<PlayerController.WeaponItem>();

    public int currentWeaponIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            EnsureSaveComponents();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void EnsureSaveComponents()
    {
        SaveableEntity entity = GetComponent<SaveableEntity>();
        if (entity == null)
            entity = gameObject.AddComponent<SaveableEntity>();

        entity.EnsureId(StableSaveId, forceOverwrite: true);

        if (GetComponent<InventoryDataSaveable>() == null)
            gameObject.AddComponent<InventoryDataSaveable>();
    }
}
