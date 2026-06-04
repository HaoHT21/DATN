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

        _animator.SetBool("Dead", true);

        _rb.linearVelocity = Vector2.zero;
        _rb.simulated = false;

        Debug.Log("PLAYER ĐÃ CHẾT! Đang chờ hồi sinh...");

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f);

        transform.position = spawnPosition;

        int before = currentHealth;
        currentHealth = maxHealth;
        IsDead = false;

        NotifyHealthChanged(before);

        _rb.simulated = true;

        _animator.SetBool("Dead", false);

        Debug.Log("ĐÃ HỒI SINH TẠI SẢNH!");
    }
}