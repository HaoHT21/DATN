using UnityEngine;



public class ItemPickup : MonoBehaviour

{

    public Sprite itemIcon;                // <--- Thêm biến này
    public int itemID;

    public GameObject weaponVisualPrefab;

    public GameObject weaponPickupPrefab;

    public GameObject bulletPrefab;

    public bool isGun;

    public int damage = 25;

    public bool isEquipped = false;

    private void OnTriggerEnter2D(Collider2D other)

    {
        if (isEquipped)
            return;

        if (other.CompareTag("Player"))

        {

            // --- ĐOẠN THÊM VÀO: NẾU ĐÂY LÀ BÌNH MÁU THÌ NHƯỜNG QUYỀN CHO HEALTHPOTION ---
            if (GetComponent<HealthPotion>() != null)
            {
                return;
            }
            // ----------------------------------------------------------------------------


            PlayerController pc = other.GetComponent<PlayerController>();

            if (pc != null)

            {

                // Truyền thêm itemIcon vào đây

                pc.PickupWeapon(weaponVisualPrefab, weaponPickupPrefab, isGun, damage, bulletPrefab, itemIcon, itemID);

                if (TryGetComponent<CollectibleSaveable>(out var collectible))
                    collectible.Collect();
                else
                    Destroy(gameObject);

            }

        }

    }

}