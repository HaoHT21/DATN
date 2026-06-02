using UnityEngine;

public class CoinMagnet : MonoBehaviour
{
    [Header("Cấu hình Coin")]
    [SerializeField] private int coinValue = 1;
    public AudioClip coinPickupSound; // Kéo file âm thanh tiếng "ting" vào đây

    [Header("Khoảng hút")]
    public float detectRange = 3f;

    [Header("Tốc độ bay")]
    public float moveSpeed = 10f;
    [SerializeField] private float acceleration = 2f;

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
        if (!isFlying) FindPlayerPhysics();
        else FlyToPlayer();
    }

    void FindPlayerPhysics()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);
        if (hit != null)
        {
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

        currentSpeed += acceleration * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, targetPlayer.position, currentSpeed * Time.deltaTime);

        float sqrDistance = (transform.position - targetPlayer.position).sqrMagnitude;
        if (sqrDistance < 0.04f)
        {
            CollectCoin();
        }
    }

    void CollectCoin()
    {
        // 1. PHÁT ÂM THANH NHẶT XU
        if (AudioManager.Instance != null && coinPickupSound != null)
        {
            AudioManager.Instance.PlaySound(coinPickupSound);
        }

        // 2. CỘNG XU VÀO STATS
        if (targetPlayer.TryGetComponent<PlayerStats>(out PlayerStats stats))
        {
            stats.AddCoin(coinValue);
            Debug.Log($"<color=yellow>[Coin]</color> Đã cộng {coinValue} xu.");
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.84f, 0f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}