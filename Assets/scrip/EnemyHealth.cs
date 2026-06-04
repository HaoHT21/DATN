using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int currentHealth = 100;
    private Animator _animator;
    private Collider2D _collider;
    private EnemyAI _enemyAI; // Thêm biến này để gọi hàm Die()

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
        _enemyAI = GetComponent<EnemyAI>(); // Lấy script EnemyAI
    }

    public void TakeDamage(int damage)
    {
        // Chặn sát thương nếu đã chết
        if (currentHealth <= 0) return;

        currentHealth -= damage;
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

        _animator?.SetBool("Death", true);

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