using UnityEngine;
using System.Collections;

public class EnemyHeath : MonoBehaviour, IHealthProvider
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;

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

        Debug.Log("Current HP: " + currentHealth);

        if (currentHealth > 0)
        {
            StartCoroutine(HurtRoutine());
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
        if (animator == null)
            yield break;

        animator.Play("hurt");

        yield return new WaitForSeconds(hurtDuration);

        animator.Play("idle");
    }

    private IEnumerator DieSequence()
    {
        isDead = true;

        OnDeath?.Invoke();

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

        EnemyShooterAI shooterAI = GetComponent<EnemyShooterAI>();
        if (shooterAI != null)
            shooterAI.enabled = false;

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