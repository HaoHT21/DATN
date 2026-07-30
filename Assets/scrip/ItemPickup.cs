using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Cấu hình dựa trên ItemData")]
    public ItemData itemData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Nếu là bình máu thì để HealthPotion xử lý
        if (GetComponent<HealthPotion>() != null) return;

        PlayerController pc = other.GetComponent<PlayerController>();

        if (pc != null && itemData != null)
        {
            pc.PickupWeapon(
                itemData.visualPrefab,
                itemData.itemPrefab, // Prefab vứt ra đất
                itemData.isGun,
                itemData.damage,
                itemData.bulletPrefab,
                itemData.itemIcon,
                itemData.itemID
            );

            if (TryGetComponent<CollectibleSaveable>(out var collectible))
                collectible.Collect();
            else
                Destroy(gameObject);
        }
    }
}
