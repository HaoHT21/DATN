using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int currentHealth = 100;
    public int maxHealth = 100;
    public bool IsDead { get; private set; }

    private Animator _animator;

    private void Awake() => _animator = GetComponent<Animator>();

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        currentHealth -= damage;
        Debug.Log($"Player bị tấn công! Máu còn: {currentHealth}");

        if (currentHealth <= 0)
        {
            PlayerDie();
        }
    }

    private void PlayerDie()
    {
        IsDead = true;
        _animator?.SetTrigger("Death"); // Giả sử animation của bạn dùng trigger "Death"
        Debug.Log("PLAYER ĐÃ HY SINH!");

        // Bạn có thể kích hoạt UI Game Over ở đây thông qua GameUIManager
        // GameUIManager.Instance.ShowGameOver();
    }
}