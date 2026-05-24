using UnityEngine;
using Fusion; // Sử dụng thư viện Fusion để nhận diện người chơi mạng

public class NPCShop : MonoBehaviour
{
    [Header("Giao diện cấu hình")]
    [SerializeField] private RectTransform shopPanel;          // Panel bảng mua súng/kiếm (Đã đổi sang RectTransform)
    [SerializeField] private GameObject interactionCanvas;  // Canvas chữ "Nhấn E để mua" trên đầu NPC

    private bool isPlayerNearby = false;

    private void Update()
    {
        // Kiểm tra an toàn đề phòng trường hợp chưa kéo thả Panel vào Inspector
        if (shopPanel == null) return;

        // Nếu người chơi ở gần và nhấn phím E
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Lấy trạng thái ẩn/hiện thông qua .gameObject chuẩn chỉ
            bool isShopOpen = shopPanel.gameObject.activeSelf;

            // Đảo ngược trạng thái của bảng cửa hàng (Đóng thành Mở, Mở thành Đóng)
            shopPanel.gameObject.SetActive(!isShopOpen);

            // Nếu đã mở bảng cửa hàng thì tạm thời ẩn dòng chữ "Nhấn E" đi cho đỡ vướng mắt
            if (shopPanel.gameObject.activeSelf)
            {
                interactionCanvas.SetActive(false);
            }
            else
            {
                interactionCanvas.SetActive(true);
            }

            // Quản lý chuột: Hiện chuột khi mở shop, ẩn chuột khi đóng shop
            Cursor.lockState = !isShopOpen ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = !isShopOpen;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Kiểm tra xem vật va chạm có phải là người chơi cục bộ (Local Player) của máy này không
        if (other.CompareTag("Player"))
        {
            // Để tránh việc người chơi khác (Client khác) đi qua NPC làm hiện chữ trên máy mình:
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.HasInputAuthority)
            {
                isPlayerNearby = true;
                interactionCanvas.SetActive(true); // Hiện dòng chữ "Nhấn E để mua"
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            NetworkObject netObj = other.GetComponent<NetworkObject>();
            if (netObj != null && netObj.HasInputAuthority)
            {
                isPlayerNearby = false;
                interactionCanvas.SetActive(false); // Ẩn dòng chữ đi khi người chơi đi xa

                if (shopPanel != null)
                {
                    shopPanel.gameObject.SetActive(false); // Tự động đóng luôn bảng cửa hàng qua .gameObject
                }
            }
        }
    }
}