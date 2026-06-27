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

    private Coroutine hurtCoroutine;

    [Header("Animation")]
    public float hurtDuration = 0.2f;

    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event System.Action<HealthChangeInfo> OnHealthChanged;
    public event System.Action OnDamaged;
    public event System.Action OnDeath;

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

        OnDamaged?.Invoke();

        Debug.Log("Current HP: " + currentHealth);

        if (currentHealth > 0)
        {
            if (hurtCoroutine != null)
                StopCoroutine(hurtCoroutine);

            hurtCoroutine = StartCoroutine(HurtRoutine());
        }
        else
        {
            currentHealth = 0;
            StartCoroutine(DieSequence());
        }

        OnHealthChanged?.Invoke(new HealthChangeInfo());
    }

    private IEnumerator HurtRoutine()
    {
        if (animator == null || isDead)
            yield break;

        animator.Play("hurt");

        yield return new WaitForSeconds(hurtDuration);

        if (!isDead)
            animator.Play("idle");

        hurtCoroutine = null;
    }

    private IEnumerator DieSequence()
    {
        isDead = true;

        OnDeath?.Invoke();

        // ========================
        // THÊM HỆ THỐNG EXP
        // =======================

        // Thưởng EXP cho Player khi quái chết
        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.AddEXP(expReward);

                Debug.Log($"Player nhận {expReward} EXP");
            }
        }

        // Tắt collider
        foreach (Collider2D col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        // Dừng vật lý
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        // Gọi animation death
        if (animator != null)
        {
            animator.Play("death");
        }

        // Tắt AI
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