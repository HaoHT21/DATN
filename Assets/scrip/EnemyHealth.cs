using System;
using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IHealthProvider
{
    public int currentHealth = 100;
    public int maxHealth = 100;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action<HealthChangeInfo> OnHealthChanged;
    private Animator _animator;
    private Collider2D _collider;
    private EnemyAI _enemyAI; // Thêm biến này để gọi hàm Die()

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
        _enemyAI = GetComponent<EnemyAI>();
        if (maxHealth < currentHealth)
            maxHealth = currentHealth;
    }

    private void NotifyHealthChanged(int previousHealth)
    {
        OnHealthChanged?.Invoke(new HealthChangeInfo(currentHealth, maxHealth, currentHealth - previousHealth));
    }

    public void TakeDamage(int damage)
    {
        // Chặn sát thương nếu đã chết
        if (currentHealth <= 0) return;

        int before = currentHealth;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        NotifyHealthChanged(before);
        _animator?.SetTrigger("Hit");

        if (currentHealth <= 0)
        {
            StartCoroutine(DieSequence());
        }
    }

    private IEnumerator DieSequence()
    {
        // Vô hiệu hóa collider để không bị va chạm khi đang chết
        if (_collider) _collider.enabled = false;

        // Dừng mọi chuyển động của kẻ địch
        if (_enemyAI != null)
        {
            _enemyAI.enabled = false; // Tắt AI để nó không đuổi theo nữa
            _enemyAI.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
        }

        _animator?.SetBool("isDead", true);

        yield return new WaitForSeconds(0.8f);

        // GỌI HÀM RƠI XU VÀ HỦY ĐỐI TƯỢNG
        if (_enemyAI != null)
        {
            _enemyAI.Die(); // Hàm này sẽ Instantiate 5 đồng xu và Destroy gameObject
        }
        else
        {
            Destroy(gameObject);
        }
    }
}