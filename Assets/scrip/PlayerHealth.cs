using System;
using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour, IHealthProvider
{
    public int currentHealth = 100;
    public int maxHealth = 100;
    public bool IsDead { get; private set; }

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<HealthChangeInfo> OnHealthChanged;

    [Header("Respawn Settings")]
    public Vector3 spawnPosition; // Vị trí điểm hồi sinh ở Sảnh (Set trong Inspector)

    private Animator _animator;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        int before = currentHealth;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        NotifyHealthChanged(before);
        Debug.Log($"Player bị tấn công! Máu còn: {currentHealth}");

        if (currentHealth <= 0)
        {
            PlayerDie();
        }
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        int before = currentHealth;
        currentHealth += amount;
        if (currentHealth > maxHealth) currentHealth = maxHealth;
        NotifyHealthChanged(before);
    }

    private void NotifyHealthChanged(int previousHealth)
    {
        OnHealthChanged?.Invoke(new HealthChangeInfo(currentHealth, maxHealth, currentHealth - previousHealth));
    }

    private void PlayerDie()
    {
        IsDead = true;
        _animator.SetTrigger("Dead"); // Kích hoạt animation Dead
        _rb.linearVelocity = Vector2.zero;   // Dừng di chuyển
        _rb.simulated = false;         // Vô hiệu hóa vật lý để không bị quái đẩy

        Debug.Log("PLAYER ĐÃ HY SINH! Đang chờ hồi sinh...");

        // Bắt đầu đếm ngược hồi sinh
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f); // Đợi 3 giây

        // Hồi sinh
        transform.position = spawnPosition; // Di chuyển về Sảnh
        int before = currentHealth;
        currentHealth = maxHealth;
        IsDead = false;
        NotifyHealthChanged(before);

        _rb.simulated = true;               // Bật lại vật lý
        _animator.Play("Idel Animation");   // Chuyển về trạng thái Idle (hoặc reset trigger)

        Debug.Log("ĐÃ HỒI SINH TẠI SẢNH!");
    }
}