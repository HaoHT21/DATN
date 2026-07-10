using UnityEngine;
using System;

public class CloneHealth : MonoBehaviour
{
    public int maxHealth = 100;
    public int currentHealth;

    public event Action OnDeath;

    bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            isDead = true;

            OnDeath?.Invoke();

            Destroy(gameObject);
        }
    }

    public void Heal(int amount)
    {
        if (isDead) return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;
    }
}