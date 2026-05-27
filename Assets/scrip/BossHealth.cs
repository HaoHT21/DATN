using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public int currentHealth = 500;
    public int maxHealth = 500;

    public void TakeDamage(int damage, string dealerTag)
    {
        if (currentHealth <= 0) return;
        currentHealth -= damage;
        if (currentHealth <= 0) GetComponent<BossAI>()?.SetDead();
    }
}