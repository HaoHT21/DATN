using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Sprite itemIcon;
    public int itemID;

    public GameObject weaponVisualPrefab;
    public GameObject weaponPickupPrefab;
    public GameObject bulletPrefab;
    public bool isGun;
    public int damage = 25;

    private void Awake()
    {
        // Bình máu tự gắn CollectibleSaveable trong HealthPotion.
        if (GetComponent<HealthPotion>() == null)
            SaveableHelpers.EnsureCollectible(gameObject, BuildStableId());
    }

    private string BuildStableId()
    {
        string scene = gameObject.scene.IsValid() ? gameObject.scene.name : "unknown";
        int x = Mathf.RoundToInt(transform.position.x * 100f);
        int y = Mathf.RoundToInt(transform.position.y * 100f);
        return $"item-pickup-{scene}-{itemID}-{x}-{y}";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Nếu đây là bình máu thì nhường quyền cho HealthPotion.
        if (GetComponent<HealthPotion>() != null)
            return;

        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc == null)
            return;

        pc.PickupWeapon(weaponVisualPrefab, weaponPickupPrefab, isGun, damage, bulletPrefab, itemIcon, itemID);

        if (TryGetComponent<CollectibleSaveable>(out var collectible))
            collectible.Collect();
        else
            Destroy(gameObject);
    }
}
