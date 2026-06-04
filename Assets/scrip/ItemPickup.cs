using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Sprite itemIcon;                // <--- Thêm biến này
    public GameObject weaponVisualPrefab;
    public GameObject weaponPickupPrefab;
    public GameObject bulletPrefab;
    public bool isGun;
    public int damage = 25;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();
            if (pc != null)
            {
                // Truyền thêm itemIcon vào đây
                pc.PickupWeapon(weaponVisualPrefab, weaponPickupPrefab, isGun, damage, bulletPrefab, itemIcon);
                Destroy(gameObject);
            }
        }
    }
}