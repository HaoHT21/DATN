using UnityEngine;
using System.Collections;

public class EnemyHeath : MonoBehaviour, IHealthProvider
{
    [Header("Health")]
    public int maxHealth = 100;
    public int currentHealth = 100;

    [Header("EXP")]
    public int expReward = 30;

    [Header("Death")]
    public float destroyDelay = 0.5f;

    [Header("Health Bar")]
    [SerializeField] private bool showHealthBar = true;

    private Rigidbody2D rb;
    private Collider2D[] colliders;

    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event System.Action<HealthChangeInfo> OnHealthChanged;
    public event System.Action OnHurt;
    public event System.Action OnDeath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0,
                maxHealth);

        if (showHealthBar && GetComponent<SimpleEnemyHealthBar>() == null)
            gameObject.AddComponent<SimpleEnemyHealthBar>();
    }

    public void TakeDamage(int damage)
    {
        if (isDead) 
            return;

        int previous = currentHealth;
        currentHealth -= damage;

        if (currentHealth > 0)
        {
            OnHurt?.Invoke();
        }
        else
        {
            currentHealth = 0;
            StartCoroutine(DieSequence());
        }

        NotifyHealthChanged(previous);
    }

    IEnumerator DieSequence()
    {
        isDead = true;

        OnDeath?.Invoke();

        GameObject player =
            GameObject.FindWithTag("Player");

        if (player != null)
        {
            PlayerHealth hp =
                player.GetComponent<PlayerHealth>();

            if (hp != null)
            {
                hp.AddEXP(expReward);
            }
        }

        foreach (Collider2D c in colliders)
        {
            c.enabled = false;
        }

        rb.linearVelocity =
            Vector2.zero;

        rb.simulated = false;

        yield return new WaitForSeconds(
            destroyDelay);

        Destroy(gameObject);
    }

    public void Heal(int amount)
    {
        if (isDead)
            return;

        int previous = currentHealth;
        currentHealth += amount;

        currentHealth =
            Mathf.Min(
                currentHealth,
                maxHealth);

        NotifyHealthChanged(previous);
    }

    private void NotifyHealthChanged(int previousHealth)
    {
        OnHealthChanged?.Invoke(
            new HealthChangeInfo(
                currentHealth,
                maxHealth,
                currentHealth - previousHealth));
    }
}