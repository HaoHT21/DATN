using UnityEngine;
using Fusion;

public class CoinMagnet : NetworkBehaviour
{
    [Header("Cấu hình Coin")]
    [SerializeField] private int coinValue = 1; // Giá trị cộng điểm của đồng xu này

    [Header("Khoảng hút")]
    public float detectRange = 3f;

    [Header("Tốc độ bay")]
    public float moveSpeed = 10f;

    private Transform targetPlayer;
    private bool isFlying = false;

    // Lớp mặt nạ vật lý để đồng xu chỉ quét đúng Layer của Player, tránh quét nhầm quái vật/tường
    [Header("Physics Settings")]
    [SerializeField] private LayerMask playerLayer;

    public override void FixedUpdateNetwork()
    {
        // Nếu chưa bay -> tìm player gần nhất bằng vòng quét vật lý mạng
        if (!isFlying)
        {
            FindPlayerPhysics();
        }
        else
        {
            FlyToPlayer();
        }
    }

    void FindPlayerPhysics()
    {
        // Sử dụng hệ thống vật lý tích hợp của Fusion để quét các Collider trong tầm hút
        Collider2D hit = Runner.GetPhysicsScene2D().OverlapCircle(transform.position, detectRange, playerLayer);

        if (hit != null)
        {
            // Kiểm tra xem đối tượng va chạm có chứa linh kiện PlayerStats hay không
            PlayerStats stats = hit.GetComponent<PlayerStats>();
            if (stats != null)
            {
                targetPlayer = hit.transform;
                isFlying = true;
            }
        }
    }

    void FlyToPlayer()
    {
        if (targetPlayer == null)
        {
            isFlying = false;
            return;
        }

        // Tịnh tiến mượt mà theo thời gian thực mạng
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPlayer.position,
            moveSpeed * Runner.DeltaTime
        );

        // Kiểm tra khoảng cách chạm ăn xu
        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (distance < 0.2f)
        {
            // Quyền tối cao thuộc về Server/Host để cộng tiền và hủy thực thể mạng
            if (Object.HasStateAuthority)
            {
                PlayerStats stats = targetPlayer.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.AddCoin(coinValue); // Tăng điểm Score mạng công bằng
                    Debug.Log($"[Coin Network] Đã cộng {coinValue} xu vào ví mạng.");
                }

                // Xóa đồng xu trên toàn bộ tất cả màn hình các Client
                Runner.Despawn(Object);
            }
        }
    }

    // Vẽ vòng tròn giả lập ngoài Scene để bạn dễ căn chỉnh khoảng cách hút trong Inspector
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gold;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}