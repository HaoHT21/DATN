using UnityEngine;

public class HealthPotion : MonoBehaviour
{
    [Header("Settings")]
    public int healAmount = 50;

    [Header("Visuals")]
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
                GameObject visual = Instantiate(potionVisualPrefab, pc.weaponHolder);
                visual.transform.localPosition = Vector3.zero; // Căn giữa tại holder
                visual.transform.localScale = new Vector3(2f, 2f, 2f); // Đồng bộ tỉ lệ với súng
                visual.SetActive(false); // Ẩn đi, chỉ hiện khi được chọn trong inventory

                // 2. Tạo item để đưa vào danh sách của Player
                PlayerController.WeaponItem newPotion = new PlayerController.WeaponItem
                {
                    visualPrefab = visual, // Gán visual vừa tạo
                    pickupPrefab = this.pickupPrefab,
                    isGun = false,         // Không phải súng
                    isPotion = true,       // Đánh dấu đây là bình máu
                    healAmount = this.healAmount
                };

                // 3. Thêm vào túi và refresh UI/Visuals
                pc.inventory.Add(newPotion);

                // Lưu ý: Đảm bảo bạn gọi hàm UpdateWeaponVisuals() trong PlayerController
                // để nó cập nhật hiển thị ngay lập tức
                Debug.Log("Đã nhặt bình máu!");

                Destroy(gameObject);
            }
        }
    }
}