using UnityEngine;

public class CoinMagnet : MonoBehaviour
{
    [Header("Cấu hình Coin")]
    [SerializeField] private int coinValue = 1;

    [Header("Khoảng hút")]
    public float detectRange = 3f;

    [Header("Tốc độ bay")]
    public float moveSpeed = 10f;
    [SerializeField] private float acceleration = 2f; // Tăng tốc để hút mượt hơn

    [Header("Physics Settings")]
    [SerializeField] private LayerMask playerLayer;

    private Transform targetPlayer;
    private bool isFlying = false;
    private float currentSpeed;

    private void Start()
    {
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
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
        // Kiểm tra vùng quanh đồng xu
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);

        if (hit != null)
        {
            // Kiểm tra xem đối tượng có script PlayerStats không
            if (hit.TryGetComponent<PlayerStats>(out PlayerStats stats))
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
            currentSpeed = moveSpeed;
            return;
        }

        // Tăng dần tốc độ khi đang bay về phía player
        currentSpeed += acceleration * Time.deltaTime;

        // Di chuyển đồng xu
        transform.position = Vector2.MoveTowards(
            transform.position,
            targetPlayer.position,
            currentSpeed * Time.deltaTime
        );

        // Kiểm tra khoảng cách bằng bình phương (sqrMagnitude) để tối ưu hiệu suất
        float sqrDistance = (transform.position - targetPlayer.position).sqrMagnitude;

        // 0.2f * 0.2f = 0.04f
        if (sqrDistance < 0.04f)
        {
            CollectCoin();
        }
    }

    void CollectCoin()
    {
        if (targetPlayer.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            stats.AddCoin(coinValue);
            Debug.Log($"<color=yellow>[Coin]</color> Đã cộng {coinValue} xu.");
        }

        // Hủy đồng xu
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Sửa lỗi Color.gold bằng mã màu RGB vàng kim
        Gizmos.color = new Color(1f, 0.84f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}