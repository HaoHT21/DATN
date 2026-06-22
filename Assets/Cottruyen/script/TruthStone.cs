using UnityEngine;

public class TruthStone : MonoBehaviour
{
    [Header("Cấu hình Hiệu ứng Luôn Hiển Thị")]
    [SerializeField] private GameObject ambientEffectPrefab; // Kéo thả Prefab hiệu ứng (lấp lánh/hào quang) vào đây

    private GameObject spawnedEffect; // Biến tạm để lưu trữ hiệu ứng sau khi sinh ra

    void Start()
    {
        // Khi game bắt đầu, tự động tạo hiệu ứng làm đẹp cho viên đá
        if (ambientEffectPrefab != null)
        {
            // Sinh ra hiệu ứng tại vị trí viên đá
            spawnedEffect = Instantiate(ambientEffectPrefab, transform.position, Quaternion.identity);

            // Ép hiệu ứng này làm CON (Child) của viên đá để viên đá di chuyển đâu, hiệu ứng đi theo đóuawd
            spawnedEffect.transform.SetParent(transform);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] Bạn chưa gắn Prefab hiệu ứng làm đẹp vào ô Ambient Effect Prefab!");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra nếu là Player chạm vào
        if (collision.CompareTag("Player"))
        {
            PlayerInventory inventory = collision.GetComponent<PlayerInventory>();

            if (inventory != null)
            {
                // 1. Cộng đá cho người chơi
                inventory.PickUpStone();

                // 2. Xóa viên đá (Vì hiệu ứng là con của viên đá nên nó sẽ bị tự động xóa theo sạch sẽ)
                Destroy(gameObject);

                Debug.Log("Đã nhặt đá và xóa toàn bộ hiệu ứng đi kèm!");
            }
        }
    }
}