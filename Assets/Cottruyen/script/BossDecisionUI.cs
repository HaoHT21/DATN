using UnityEngine;
using UnityEngine.UI;

public class BossDecisionUI : MonoBehaviour
{
    public static BossDecisionUI Instance { get; private set; }

    [Header("Cấu hình các nút bấm UI")]
    public Button executeButton;
    public Button spareButton;

    private BossDecisionTrigger _currentTrigger;

    private void Awake()
    {
        // Khởi tạo Singleton đảm bảo các script khác luôn truy cập được qua Instance
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Mặc định ẩn bảng UI này đi khi mới bắt đầu game
        gameObject.SetActive(false);
    }

    // Hàm mở UI nhận tham chiếu từ script Trigger độc lập bất kỳ gửi tới
    public void ShowTriggerDecision(BossDecisionTrigger trigger)
    {
        _currentTrigger = trigger;

        // Hiện Canvas UI lên màn hình
        gameObject.SetActive(true);

        // Dừng thời gian hệ thống game để người chơi lựa chọn an toàn
        Time.timeScale = 0f;

        // Xóa sạch các sự kiện lắng nghe cũ để tránh trùng lặp lệnh bấm nút
        executeButton.onClick.RemoveAllListeners();
        spareButton.onClick.RemoveAllListeners();

        // Gán sự kiện click mới cho hai nút
        executeButton.onClick.AddListener(OnExecuteSelected);
        spareButton.onClick.AddListener(OnSpareSelected);
    }

    void OnExecuteSelected()
    {
        // Khôi phục lại thời gian thực cho game (Bắt buộc trước khi chạy Coroutine)
        Time.timeScale = 1f;
        gameObject.SetActive(false);

        if (_currentTrigger != null)
        {
            _currentTrigger.ConfirmExecute(); // Ra lệnh thực hiện nhánh Kết Liễu
        }
    }

    void OnSpareSelected()
    {
        Time.timeScale = 1f;
        gameObject.SetActive(false);

        if (_currentTrigger != null)
        {
            _currentTrigger.ConfirmSpare(); // Ra lệnh thực hiện nhánh Tha Mạng
        }
    }
}