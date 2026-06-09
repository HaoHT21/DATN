using UnityEngine;

public class BossEnemyHealth : MonoBehaviour
{
    public int currentHealth = 500;
    public int maxHealth = 500;
    public int armor = 0;

    private BossEnemyAI _bossAI;

    private void Awake()
    {
        _bossAI = GetComponent<BossEnemyAI>();
    }

    public void TakeDamage(int damage, string dealerTag = "")
    {
        if (_bossAI != null && _bossAI.IsDead) return;
        if (currentHealth <= 0) return;

        int finalDamage = Mathf.Max(1, damage - armor);
        currentHealth -= finalDamage;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            _bossAI?.EnterDeath();
            return;
        }

        _bossAI?.RequestHurt();
    }
}
