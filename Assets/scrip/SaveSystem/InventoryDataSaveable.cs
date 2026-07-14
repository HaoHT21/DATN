using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SaveableEntity))]
public sealed class InventoryDataSaveable : MonoBehaviour, ISaveable
{
    [Serializable]
    private sealed class ItemEntry
    {
        public int itemID;
        public bool isGun;
        public int damage;
        public bool isPotion;
        public int healAmount;
    }

    [Serializable]
    private sealed class State
    {
        public int currentWeaponIndex;
        // Giữ field cũ để đọc save đã tạo trước khi thêm ItemEntry.
        public List<int> itemIds = new List<int>();
        public List<ItemEntry> items = new List<ItemEntry>();
    }

    private struct ItemTemplate
    {
        public Sprite icon;
        public GameObject visualPrefab;
        public GameObject pickupPrefab;
        public GameObject bulletPrefab;
        public bool isGun;
        public int damage;
        public bool isPotion;
        public int healAmount;
    }

    public object CaptureState()
    {
        if (InventoryData.Instance == null || InventoryData.Instance.sharedInventory == null)
            return null;

        var state = new State
        {
            currentWeaponIndex = InventoryData.Instance.currentWeaponIndex
        };

        foreach (var item in InventoryData.Instance.sharedInventory)
        {
            if (item == null)
                continue;

            state.itemIds.Add(item.itemID);
            state.items.Add(new ItemEntry
            {
                itemID = item.itemID,
                isGun = item.isGun,
                damage = item.damage,
                isPotion = item.isPotion,
                healAmount = item.healAmount
            });
        }

        return state;
    }

    public void RestoreState(object state)
    {
        if (state is not State s)
            return;

        if (InventoryData.Instance == null)
            return;

        PlayerController player = UnityEngine.Object.FindFirstObjectByType<PlayerController>();
        ClearExistingVisuals(player);

        InventoryData.Instance.sharedInventory.Clear();
        InventoryData.Instance.currentWeaponIndex = Mathf.Max(0, s.currentWeaponIndex);

        List<ItemEntry> entries = BuildEntries(s);
        for (int i = 0; i < entries.Count; i++)
        {
            ItemEntry entry = entries[i];
            if (entry == null)
                continue;

            ItemTemplate template = ResolveTemplate(entry);
            GameObject spawnedVisual = SpawnVisual(player, template.visualPrefab, template.isPotion);

            InventoryData.Instance.sharedInventory.Add(new PlayerController.WeaponItem
            {
                itemID = entry.itemID,
                icon = template.icon,
                visualPrefab = spawnedVisual,
                pickupPrefab = template.pickupPrefab,
                isGun = template.isGun,
                damage = template.damage,
                bulletPrefab = template.bulletPrefab,
                isPotion = template.isPotion,
                healAmount = template.healAmount
            });
        }

        if (InventoryData.Instance.sharedInventory.Count > 0)
        {
            InventoryData.Instance.currentWeaponIndex = Mathf.Clamp(
                InventoryData.Instance.currentWeaponIndex,
                0,
                InventoryData.Instance.sharedInventory.Count - 1);
        }
        else
        {
            InventoryData.Instance.currentWeaponIndex = 0;
        }

        if (player != null)
            player.UpdateWeaponVisuals();
    }

    private static List<ItemEntry> BuildEntries(State s)
    {
        if (s.items != null && s.items.Count > 0)
            return s.items;

        var entries = new List<ItemEntry>();
        if (s.itemIds == null)
            return entries;

        for (int i = 0; i < s.itemIds.Count; i++)
        {
            entries.Add(new ItemEntry { itemID = s.itemIds[i] });
        }

        return entries;
    }

    private static void ClearExistingVisuals(PlayerController player)
    {
        if (InventoryData.Instance != null && InventoryData.Instance.sharedInventory != null)
        {
            foreach (var item in InventoryData.Instance.sharedInventory)
            {
                if (item != null && item.visualPrefab != null)
                    UnityEngine.Object.Destroy(item.visualPrefab);
            }
        }

        // Không xóa toàn bộ children của weaponHolder — FirePoint/FP nằm trong đó.
        if (player != null)
        {
            player.ClearWeaponVisualsKeepMounts();
            player.EnsureWeaponMounts();
        }
    }

    private static GameObject SpawnVisual(PlayerController player, GameObject visualPrefab, bool isPotion)
    {
        if (player == null || player.weaponHolder == null || visualPrefab == null)
            return null;

        GameObject spawned = UnityEngine.Object.Instantiate(visualPrefab, player.weaponHolder);
        spawned.transform.localPosition = Vector3.zero;
        spawned.transform.localRotation = Quaternion.identity;

        if (isPotion)
            spawned.transform.localScale = new Vector3(2f, 2f, 2f);
        else
            spawned.transform.localScale = visualPrefab.transform.localScale;

        // Prefab súng thường self-reference (weaponVisualPrefab = chính object nhặt).
        // Khi load, object gốc đã bị CollectibleSaveable tắt renderer → copy cũng vô hình.
        SanitizeHeldVisual(spawned);

        spawned.SetActive(false);
        return spawned;
    }

    /// <summary>
    /// Loại bỏ logic nhặt đồ / collider và bật lại renderer cho visual cầm trên tay.
    /// </summary>
    private static void SanitizeHeldVisual(GameObject spawned)
    {
        if (spawned == null)
            return;

        foreach (CollectibleSaveable c in spawned.GetComponentsInChildren<CollectibleSaveable>(true))
            UnityEngine.Object.Destroy(c);

        foreach (SaveableEntity c in spawned.GetComponentsInChildren<SaveableEntity>(true))
            UnityEngine.Object.Destroy(c);

        foreach (ItemPickup c in spawned.GetComponentsInChildren<ItemPickup>(true))
            UnityEngine.Object.Destroy(c);

        foreach (HealthPotion c in spawned.GetComponentsInChildren<HealthPotion>(true))
            UnityEngine.Object.Destroy(c);

        foreach (Collider2D c in spawned.GetComponentsInChildren<Collider2D>(true))
            UnityEngine.Object.Destroy(c);

        foreach (Collider c in spawned.GetComponentsInChildren<Collider>(true))
            UnityEngine.Object.Destroy(c);

        SpriteRenderer[] sprites = spawned.GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].enabled = true;
        }

        Renderer[] renderers = spawned.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                renderers[i].enabled = true;
        }
    }

    private static ItemTemplate ResolveTemplate(ItemEntry entry)
    {
        var template = new ItemTemplate
        {
            isGun = entry.isGun,
            damage = entry.damage,
            isPotion = entry.isPotion,
            healAmount = entry.healAmount
        };

        if (entry.isPotion)
        {
            TryResolveFromHealthPotion(entry.itemID, ref template);
            template.isPotion = true;
            template.isGun = false;
            if (entry.healAmount > 0)
                template.healAmount = entry.healAmount;
            return template;
        }

        if (TryResolveFromHealthPotion(entry.itemID, ref template))
            return template;

        if (TryResolveFromItemPickup(entry.itemID, ref template))
            return template;

        if (TryResolveFromItemData(entry.itemID, ref template))
            return template;

        return template;
    }

    private static bool TryResolveFromHealthPotion(int itemId, ref ItemTemplate template)
    {
        HealthPotion[] potions = UnityEngine.Object.FindObjectsByType<HealthPotion>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < potions.Length; i++)
        {
            HealthPotion potion = potions[i];
            if (potion == null || potion.itemID != itemId)
                continue;

            template.icon = potion.itemIcon;
            template.visualPrefab = potion.potionVisualPrefab;
            template.pickupPrefab = potion.pickupPrefab;
            template.isGun = false;
            template.isPotion = true;
            template.healAmount = potion.healAmount > 0 ? potion.healAmount : template.healAmount;
            template.damage = 0;
            template.bulletPrefab = null;
            return true;
        }

        return false;
    }

    private static bool TryResolveFromItemPickup(int itemId, ref ItemTemplate template)
    {
        ItemPickup[] pickups = UnityEngine.Object.FindObjectsByType<ItemPickup>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        for (int i = 0; i < pickups.Length; i++)
        {
            ItemPickup pickup = pickups[i];
            if (pickup == null || pickup.itemID != itemId)
                continue;

            if (pickup.GetComponent<HealthPotion>() != null)
                continue;

            template.icon = pickup.itemIcon;
            template.visualPrefab = pickup.weaponVisualPrefab;
            template.pickupPrefab = pickup.weaponPickupPrefab;
            template.bulletPrefab = pickup.bulletPrefab;
            template.isGun = pickup.isGun;
            template.damage = pickup.damage;
            template.isPotion = false;
            template.healAmount = 0;
            return true;
        }

        return false;
    }

    private static bool TryResolveFromItemData(int itemId, ref ItemTemplate template)
    {
        ItemData data = FindItemDataById(itemId);
        if (data == null)
            return false;

        template.icon = data.itemIcon;
        template.visualPrefab = data.visualPrefab;
        template.pickupPrefab = data.itemPrefab;
        template.bulletPrefab = data.bulletPrefab;
        template.isGun = data.isGun;
        template.damage = data.damage;
        return true;
    }

    private static ItemData FindItemDataById(int id)
    {
        if (ShopManager.Instance != null)
        {
            try
            {
                var field = typeof(ShopManager).GetField("allShopItems",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (field != null)
                {
                    var list = field.GetValue(ShopManager.Instance) as List<ItemData>;
                    if (list != null)
                    {
                        ItemData found = list.Find(x => x != null && x.itemID == id);
                        if (found != null)
                            return found;
                    }
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
