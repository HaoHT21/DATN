using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Cấu hình Phím tắt")]
    public KeyCode toggleKey = KeyCode.Q; // Phím Q để bật/tắt kho đồ lớn

    [Header("Các Panel UI")]
    public GameObject kholonPanel;       // Kéo thả Object "Kholon" vào đây

    [Header("Mảng các Image hiển thị Icon")]
    public Image[] iconDisplays;          // Kéo tất cả các ô icon (cả hotbar và bảng lớn nếu muốn) vào đây

    [Header("Cấu hình hiển thị Nhân vật")]
    public Image playerAvatarDisplay;     // Kéo thả Image "Bảng Player" vào đây

    private PlayerController player;
    private Image[] slotBackgrounds;
    private bool isKholonOpen = false;    // Trạng thái đóng/mở của kho lớn

    void Start()
    {
        // Khởi tạo mảng lưu background của các ô
        slotBackgrounds = new Image[iconDisplays.Length];
        for (int i = 0; i < iconDisplays.Length; i++)
        {
            if (iconDisplays[i] != null && iconDisplays[i].transform.parent != null)
            {
                slotBackgrounds[i] = iconDisplays[i].transform.parent.GetComponent<Image>();
            }
        }

        // Mặc định lúc vào game sẽ ẩn bảng kho đồ lớn đi
        if (kholonPanel != null)
        {
            kholonPanel.SetActive(false);
            isKholonOpen = false;
        }

        FindAndSetupPlayer();
    }

    void Update()
    {
        // 1. XỬ LÝ NHẤN PHÍM Q ĐỂ BẬT/TẮT KHOLON
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleKholon();
        }

        // 2. THEO DÕI ĐỔI NHÂN VẬT
        PlayerController activePlayer = FindFirstObjectByType<PlayerController>();
        if (activePlayer != null && activePlayer != player)
        {
            if (player != null)
            {
                player.OnInventoryChanged -= UpdateUI;
            }

            player = activePlayer;
            player.OnInventoryChanged += UpdateUI;

            UpdateUI();
        }
    }

    // Hàm thực hiện việc bật/tắt Panel Kholon
    void ToggleKholon()
    {
        if (kholonPanel == null) return;

        isKholonOpen = !isKholonOpen;
        kholonPanel.SetActive(isKholonOpen);

        // Mỗi lần mở kho đồ lên, ép giao diện cập nhật mới nhất ngay lập tức
        if (isKholonOpen)
        {
            UpdateUI();
        }
    }

    void FindAndSetupPlayer()
    {
        player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            player.OnInventoryChanged += UpdateUI;
            UpdateUI();
        }
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnInventoryChanged -= UpdateUI;
        }
    }

    public void UpdateUI()
    {
        // 1. ĐỒNG BỘ ẢNH ĐẠI DIỆN PLAYER
        if (player != null && playerAvatarDisplay != null)
        {
            if (player.playerAvatar != null)
            {
                playerAvatarDisplay.sprite = player.playerAvatar;
                playerAvatarDisplay.enabled = true;
            }
            else
            {
                playerAvatarDisplay.enabled = false;
            }
        }

        // 2. ĐỒNG BỘ CÁC Ô VẬT PHẨM (HOTBAR + KHO LỚN)
        if (InventoryData.Instance == null || InventoryData.Instance.sharedInventory == null) return;
        var inv = InventoryData.Instance.sharedInventory;

        // --- CHỖ ĐÃ SỬA: ĐỊNH NGHĨA LẠI MÀU SẮC ĐỂ TRÁNH LỖI TÀNG HÌNH Ô NỀN ---
        Color highlightColor = new Color(1f, 0.92f, 0.01f, 1f); // Màu vàng hiện rõ khi được chọn làm vũ khí chính
        Color normalColor = new Color(0f, 0f, 0f, 0f);          // ĐỔI THÀNH 1f: Ô có đồ nhưng không chọn vẫn hiện nền gỗ rõ ràng 100%
        Color emptyColor = new Color(0f, 0f, 0f, 0f);           // ĐỔI THÀNH 1f: Khung ô gỗ trống trải vẫn giữ nguyên để không bị tàng hình

        for (int i = 0; i < iconDisplays.Length; i++)
        {
            if (iconDisplays[i] == null) continue;

            if (i < inv.Count)
            {
                // Nếu ô có đồ, gán ảnh vũ khí, đặt Alpha của icon vũ khí rõ nét và bật nó lên
                iconDisplays[i].sprite = inv[i].icon;
                iconDisplays[i].enabled = (inv[i].icon != null);
                iconDisplays[i].color = new Color(1f, 1f, 1f, 1f);

                // Đổi màu ô nền dựa vào món đồ có đang được chọn hay không
                if (slotBackgrounds[i] != null)
                {
                    slotBackgrounds[i].color = (i == InventoryData.Instance.currentWeaponIndex) ? highlightColor : normalColor;
                }
            }
            else
            {
                // Nếu là ô trống, ẩn ảnh icon vũ khí đi (nhưng giữ nguyên khung gỗ bên dưới)
                iconDisplays[i].sprite = null;
                iconDisplays[i].enabled = false;

                if (slotBackgrounds[i] != null)
                {
                    slotBackgrounds[i].color = emptyColor; // Giữ khung gỗ bình thường
                }
            }
            if (i == 0 && i < inv.Count && inv[0] != null && inv[0].icon != null)
            {
                Debug.Log("Ô đầu tiên đang chứa icon tên là: " + inv[0].icon.name);
            }
        }
    }
}