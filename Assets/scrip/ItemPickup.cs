using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public GameObject weaponVisualPrefab; // Prefab hiển thị trên tay (file Project)
    public GameObject weaponPickupPrefab; // File Prefab gốc (file Project)
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
                // Truyền trực tiếp các Prefab từ file Project
                pc.PickupWeapon(weaponVisualPrefab, weaponPickupPrefab, isGun, damage, bulletPrefab);
                Destroy(gameObject); // Xóa vật phẩm trên bản đồ
            }
        }
    }
}