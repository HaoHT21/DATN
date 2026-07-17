using UnityEngine;

public class VolumeEscManager : MonoBehaviour
{
    [Header("--- CẤU HÌNH PHÍM ESC ---")]
    [Tooltip("Kéo trực tiếp GameObject Settings_Panel từ Hierarchy vào đây")]
    public GameObject settingsPanel;

    private VolumeSettings _volumeSettings;

    private void Start()
    {
        // Tự động tìm kiếm script VolumeSettings đang nằm trên Settings_Panel (nếu có)
        if (settingsPanel != null)
        {
            _volumeSettings = settingsPanel.GetComponent<VolumeSettings>();

            // Đảm bảo lúc mới vào game, bảng Cài Đặt luôn ẩn đi gọn gàng
            settingsPanel.SetActive(false);
        }
    }

    private void Update()
    {
        // Bắt sự kiện khi người dùng nhấn nút ESC trên bàn phím
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }
    }

    public void ToggleSettings()
    {
        if (settingsPanel == null)
        {
            Debug.LogWarning("Chưa kéo Settings_Panel vào VolumeEscManager đâu nhé Trung ơi!");
            return;
        }

        // Đảo ngược trạng thái hiển thị (Đang mở -> Đóng, Đang đóng -> Mở)
        bool isActive = !settingsPanel.activeSelf;
        settingsPanel.SetActive(isActive);

        // XỬ LÝ TIME SCALE: Tạm dừng toàn bộ game khi mở bảng, chạy tiếp khi đóng bảng
        if (isActive)
        {
            Time.timeScale = 0f; // Ngừng mọi chuyển động (quái đứng im, đạn ngừng bay, bẫy dừng chạy)
        }
        else
        {
            Time.timeScale = 1f; // Game hoạt động bình thường trở lại
        }
    }

    // Hàm bổ trợ cực kỳ quan trọng cho nút Đóng (X) trên UI click vào
    public void CloseSettingsViaButton()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
            Time.timeScale = 1f; // Trả lại thời gian thực cho game hoạt động bình thường
        }
    }
}