using UnityEngine;
using UnityEngine.SceneManagement; // BẮT BUỘC: Để chuyển Scene

public class BadEndingUIController : MonoBehaviour
{
    [Header("Cấu hình Tên Scene")]
    public string menuSceneName = "Menu"; // Đặt tên Scene Menu của bạn vào đây

    // Hàm gắn cho Nút quay về Menu
    public void LoadMenuScene()
    {
        // Trả lại thời gian bình thường cho hệ thống trước khi chuyển Scene
        Time.timeScale = 1f;

        Debug.Log($"[BadEndUI] Đang tải về Scene: {menuSceneName}");
        SceneManager.LoadScene(menuSceneName);
    }

    // Hàm gắn cho Nút quay lại chỗ spawn (Hồi sinh)
    public void RespawnPlayer()
    {
        // 1. Tìm đối tượng Player trên Scene thông qua Tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Trả lại thời gian bình thường để game chạy tiếp
                Time.timeScale = 1f;

                // 2. Tắt trạng thái đứng trong Zone 2 nguy hiểm để cho phép hồi sinh
                playerHealth.SetInBadEndZone(false, null);

                // 3. Đưa máu Player về lại mức bình thường thông qua hàm Heal (hoặc cấu hình lại máu)
                // Vì IsDead đang là true, chúng ta cần reset các trạng thái của Player thông qua việc hồi sinh.
                // Để sạch sẽ nhất, chúng ta sẽ ép Player gọi lại chuỗi Routine hồi sinh của chính nó bằng cách kích hoạt một hàm gián tiếp hoặc hồi máu:

                // Vì hàm RespawnRoutine trong PlayerHealth là private, cách tối ưu nhất mà không sửa code cũ 
                // là chúng ta gọi một hàm xử lý hồi sinh nhanh tại đây:
                ForceResetPlayer(player, playerHealth);

                // 4. Ẩn bảng UI Bad Ending này đi
                gameObject.SetActive(false);

                Debug.Log("[BadEndUI] Đã hồi sinh Player thành công và đưa về Sảnh!");
            }
            else
            {
                Debug.LogError("[BadEndUI] Không tìm thấy script PlayerHealth trên Player!");
            }
        }
        else
        {
            Debug.LogError("[BadEndUI] Không tìm thấy GameObject nào có Tag là 'Player' trên Scene!");
        }
    }

    // Hàm bổ trợ giải phóng bùa chú "Chết" của PlayerHealth mà không làm thay đổi code gốc của bạn
    private void ForceResetPlayer(GameObject player, PlayerHealth health)
    {
        // Đưa vị trí về điểm hồi sinh ở Sảnh
        player.transform.position = health.spawnPosition;

        // Bật lại vật lý
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = true;

        // Tắt hoạt ảnh chết
        Animator anim = player.GetComponent<Animator>();
        if (anim != null) anim.SetBool("Dead", false);

        // Hồi đầy máu bằng cách tận dụng hàm Heal có sẵn của bạn (hoặc gọi qua hệ thống)
        // Vì IsDead đang khóa hàm Heal, chúng ta sẽ dùng Reflection hoặc kích hoạt gián tiếp, 
        // nhưng cách đơn giản nhất là can thiệp biến thông qua sửa đổi nhỏ hoặc ép trạng thái:

        // Để đảm bảo ăn khớp tuyệt đối với biến IsDead (đang bị private set), bạn chỉ cần đảm bảo 
        // khi nhấn nút này, game sẽ Load lại chính Scene hiện tại là cách an toàn và sạch sẽ nhất cho các biến dữ liệu:

        // TÙY CHỌN AN TOÀN NHẤT: Khởi động lại màn chơi hiện tại để tránh lỗi dữ liệu rác
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}