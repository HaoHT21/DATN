using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Settings")]
    public int healAmount = 50;
    public int itemID;

    [Header("Visuals")]
    public Sprite itemIcon;               // Kéo ảnh sprite bình máu vào đây trên Inspector
    public GameObject potionVisualPrefab; // Kéo Prefab hình ảnh bình máu của bạn vào đây
    public GameObject pickupPrefab;       // File prefab gốc của bình máu (để khi vứt ra)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController pc = other.GetComponent<PlayerController>();

            if (pc != null)
            {
                // 1. Tạo hình ảnh bình máu gắn vào tay nhân vật (ẩn đi ban đầu)
                GameObject visual = null;
                if (potionVisualPrefab != null && pc.weaponHolder != null)
                {
                    visual = Instantiate(potionVisualPrefab, pc.weaponHolder);
                    visual.transform.localPosition = Vector3.zero; // Căn giữa tại holder
                    visual.transform.localScale = new Vector3(2f, 2f, 2f); // Đồng bộ tỉ lệ với súng
                    visual.SetActive(false); // Ẩn đi, chỉ hiện khi được chọn trong inventory
                }

                // 2. Tạo item để đưa vào danh sách của Player
                PlayerController.WeaponItem newPotion = new PlayerController.WeaponItem
                {
                    itemID = this.itemID,
                    icon = this.itemIcon, // Gán icon để hiển thị chính xác trong Kholon
                    visualPrefab = visual, // Gán visual vừa tạo
                    pickupPrefab = this.pickupPrefab,
                    isGun = false,         // Không phải súng
                    isPotion = true,       // Đánh dấu đây là bình máu
                    healAmount = this.healAmount
                };

                // 3. Thêm thẳng vào list inventory dùng chung của PlayerController
                if (pc.inventory != null)
                {
                    pc.inventory.Add(newPotion);
                    pc.UpdateWeaponVisuals(); // Cập nhật và kích hoạt OnInventoryChanged làm mới UI
                }

                Debug.Log("Đã nhặt bình máu thành công!");
                if (TryGetComponent<CollectibleSaveable>(out var collectible))
                    collectible.Collect();
                else
                    Destroy(gameObject);
            }
        }
    }
}