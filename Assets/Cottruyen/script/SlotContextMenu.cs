using UnityEngine;
using UnityEngine.UI;

public class SlotContextMenu : MonoBehaviour
{
    public static SlotContextMenu Instance { get; private set; }

    [Header("Cấu hình nút bấm")]
    public Button btnUse;
    public Button btnDrop;

    private int currentTargetSlotIndex = -1;

    void Awake()
    {
        Instance = this;
        gameObject.SetActive(false); // Mặc định ẩn menu này đi khi vào game
    }

    void Update()
    {
        // Nếu click chuột trái ra ngoài khi menu đang mở thì tự đóng menu lại
        if (Input.GetMouseButtonDown(0) && !RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(), Input.mousePosition))
        {
            HideMenu();
        }
    }

    public void ShowMenu(int slotIndex, Vector2 mousePosition)
    {
        currentTargetSlotIndex = slotIndex;
        gameObject.SetActive(true);

        // Đặt vị trí của bảng Menu ngay tại vị trí con trỏ chuột
        transform.position = mousePosition;

        // Xóa các sự kiện cũ trước khi gán mới để tránh chồng chéo listeners
        btnUse.onClick.RemoveAllListeners();
        btnDrop.onClick.RemoveAllListeners();

        // Đăng ký sự kiện click chuột trái cho 2 nút
        btnUse.onClick.AddListener(() => OnUseClicked());
        btnDrop.onClick.AddListener(() => OnDropClicked());
    }

    public void HideMenu()
    {
        gameObject.SetActive(false);
        currentTargetSlotIndex = -1;
    }

    void OnUseClicked()
    {
        if (currentTargetSlotIndex == -1) return;

        // Tìm Player đang hoạt động trong Scene để ra lệnh đổi đồ
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.SwitchToWeaponIndex(currentTargetSlotIndex);
        }

        HideMenu();
    }

    void OnDropClicked()
    {
        if (currentTargetSlotIndex == -1) return;

        // Tìm Player đang hoạt động trong Scene để ra lệnh vứt đồ
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.DropWeaponAtSlot(currentTargetSlotIndex);
        }

        HideMenu();
    }
}