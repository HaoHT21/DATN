using UnityEngine;

public class CoinMagnet : MonoBehaviour
{
    [Header("Cấu hình Coin")]
    [SerializeField] private int coinValue = 1;

    [Header("Khoảng hút")]
    public float detectRange = 3f;

    [Header("Tốc độ bay")]
    public float moveSpeed = 10f;

    [Header("Physics Settings")]
    [SerializeField] private LayerMask playerLayer;

    private Transform targetPlayer;
    private bool isFlying = false;

    private void Update()
    {
        // Sử dụng Update thay vì FixedUpdateNetwork
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
        // Dùng Physics2D chuẩn của Unity
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);

        if (hit != null)
        {
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

        // Tịnh tiến mượt mà theo Time.deltaTime
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPlayer.position,
            moveSpeed * Time.deltaTime
        );

        // Kiểm tra khoảng cách chạm ăn xu
        float distance = Vector2.Distance(transform.position, targetPlayer.position);

        if (distance < 0.2f)
        {
            // Trong chế độ Offline, không cần kiểm tra HasStateAuthority
            PlayerStats stats = targetPlayer.GetComponent<PlayerStats>();
            if (stats != null)
            {
                stats.AddCoin(coinValue);
                Debug.Log($"[Coin Offline] Đã cộng {coinValue} xu vào ví.");
            }

            // Hủy đồng xu ngay lập tức
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gold;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}