using UnityEngine;
using System.Collections;

public class EnemyHeath : MonoBehaviour, IHealthProvider
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("--- PHẦN THƯỞNG KHI CHẾT (MỚI TÍCH HỢP) ---")]
    [Tooltip("Lượng EXP sẽ thưởng cho Player khi con quái này chết")]
    public int expReward = 30;

    [Header("Death")]
    public float destroyDelay = 0.2f;

    private Animator animator;
    private Collider2D[] colliders;
    private Rigidbody2D rb;

    [Header("Animation")]
    public float hurtDuration = 0.2f;

    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event System.Action<HealthChangeInfo> OnHealthChanged;
    public System.Action OnDeath;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;

        Debug.Log(gameObject.name + " Current HP: " + currentHealth);

        if (currentHealth > 0)
        {
            StartCoroutine(HurtRoutine());
        }
        else
        {
            currentHealth = 0;
            StartCoroutine(DieSequence());
        }

        // Truyền cấu hình rỗng tránh lỗi interface cũ của nhóm
        OnHealthChanged?.Invoke(new HealthChangeInfo());
    }

    private IEnumerator HurtRoutine()
    {
        if (animator == null)
            yield break;

        animator.Play("hurt");

        yield return new WaitForSeconds(hurtDuration);

        // Đề phòng quái chết rồi thì không chơi lại animation idle tránh bị khựng xác
        if (!isDead)
        {
            animator.Play("idle");
        }
    }

    private IEnumerator DieSequence()
    {
        isDead = true;

        OnDeath?.Invoke();

        // ====================================================================
        // LOGIC THƯỞNG EXP: Tìm Player để bơm điểm kinh nghiệm thăng cấp
        // ====================================================================
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.AddEXP(expReward); // Gọi hàm cộng EXP và check lên cấp của mày
                Debug.Log($"<color=cyan>[Hệ thống]</color> Đã thưởng {expReward} EXP từ {gameObject.name} cho Player!");
            }
        }

        // ====================================================================
        // LOGIC RƠI XU (COIN DROP): Gọi script rớt tiền của nhóm nếu có gán trên quái
        // ====================================================================
        CoinDrop coinScript = GetComponent<CoinDrop>();
        if (coinScript != null)
        {
            // Mày kiểm tra xem script CoinDrop của nhóm dùng hàm gì để rơi xu 
            // Thường là hàm DropCoin() hoặc Drop(), tao gõ sẵn lệnh chạy ở đây:
            coinScript.enabled = true;
            // Nếu nhóm viết hàm tự động rơi trong Start/OnDestroy thì không cần gọi, 
            // còn nếu gọi bằng tay thì mở comment dòng dưới ra:
            // coinScript.DropCoin(); 
        }

        // Tắt hoàn toàn các collider vật lý để đạn súng và Player đi xuyên qua xác
        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        // Dừng hoàn toàn chuyển động vật lý 2D
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Gọi animation death nằm xuống nổ xác
        if (animator != null)
        {
            animator.Play("death");
        }

        // Tắt AI di chuyển và bắn đạn của quái để nó không cắn lén lúc chết
        EnemyShooterAI shooterAI = GetComponent<EnemyShooterAI>();
        if (shooterAI != null)
            shooterAI.enabled = false;

        // Chờ chơi xong animation chết rồi xóa sổ khỏi game
        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(new HealthChangeInfo());
    }
}