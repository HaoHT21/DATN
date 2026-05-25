using UnityEngine;
using Fusion; // Bắt buộc phải có thư viện này để dùng Fusion

public class DoorTeleport : MonoBehaviour
{
    [Header("Điểm dịch chuyển đến")]
    [SerializeField] private Transform spawnPoint;

    // Sửa thành OnTriggerEnter2D vì game của bạn là cấu trúc 2D
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Kiểm tra va chạm có phải là Nhân vật chơi mạng hay không
        NetworkObject networkPlayer = other.GetComponent<NetworkObject>();

        if (networkPlayer != null && other.CompareTag("Player"))
        {
            // 2. CHỈ MÁY HOST (Có State Authority) mới được phép quyết định dịch chuyển
            // Điều này tránh lỗi desync giật hình và chống hack vị trí từ Client
            if (networkPlayer.Runner.IsServer)
            {
                // Kiểm tra nếu nhân vật có gắn kèm bộ điều khiển vị trí mạng NetworkTransform
                if (networkPlayer.TryGetComponent<NetworkTransform>(out var networkTransform))
                {
                    // Cách dịch chuyển CHUẨN của Photon Fusion: 
                    // Phải gọi hàm Teleport của NetworkTransform thay vì gán transform.position trực tiếp
                    networkTransform.Teleport(spawnPoint.position);

                    Debug.Log($"[Mạng] Đã dịch chuyển thành công người chơi {networkPlayer.Id} tới điểm mới.");
                }
                else
                {
                    // Nếu nhân vật chỉ dùng Rigidbody2D mạng di chuyển thông thường
                    other.transform.position = spawnPoint.position;
                }
            }
        }
    }
}