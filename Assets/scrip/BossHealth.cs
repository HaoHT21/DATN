using System;
using UnityEngine;

public class BossHealth : MonoBehaviour, IHealthProvider
{
    public int currentHealth = 500;
    public int maxHealth = 500;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    public event Action<HealthChangeInfo> OnHealthChanged;

    public void TakeDamage(int damage, string dealerTag)
    {
        if (currentHealth <= 0) return;
        int before = currentHealth;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;
        OnHealthChanged?.Invoke(new HealthChangeInfo(currentHealth, maxHealth, currentHealth - before));
        if (currentHealth <= 0) GetComponent<BossAI>()?.SetDead();
    }
}