using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IHealthProvider
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("Death")]
    public float destroyDelay = 1f;

    private Animator animator;
    private Collider2D[] colliders;
    private Rigidbody2D rb;

    private EnemyAI enemyAI;

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

        enemyAI = GetComponent<EnemyAI>();

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        Debug.Log("Current HP: " + currentHealth);

        if (currentHealth > 0)
        {
            enemyAI?.PlayHurt();
        }

        if (currentHealth <= 0)
        {
            Debug.Log("Enemy Dead");
            StartCoroutine(DieSequence());
        }
    }

    private IEnumerator DieSequence()
    {
        isDead = true;

        OnDeath?.Invoke();

        foreach (var col in colliders)
        {
            if (col != null)
                col.enabled = false;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        EnemyAI ai = GetComponent<EnemyAI>();

        if (ai != null)
            ai.enabled = false;

        if (animator != null)
        {
            animator.Play("death");
        }

        yield return new WaitForSeconds(destroyDelay);

        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(new HealthChangeInfo());
    }
}