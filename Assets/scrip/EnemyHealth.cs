using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IHealthProvider
{
    public int currentHealth = 100;
    public int maxHealth = 100; // Cần có maxHealth để UI tính tỷ lệ % thanh máu đầy/vơi

    private Animator _animator;
    private Collider2D _collider;
    private EnemyAI _enemyAI;
    private RangedEnemyAI _rangedEnemyAI;

    // =================================================================
    // THỰC THI INTERFACE IHEALTHPROVIDER BẮT BUỘC CỦA DỰ ÁN
    // =================================================================
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    // Khai báo Event OnHealthChanged theo đúng kiểu HealthChangeInfo mà hệ thống yêu cầu
    public event System.Action<HealthChangeInfo> OnHealthChanged;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
        _enemyAI = GetComponent<EnemyAI>();
        _rangedEnemyAI = GetComponent<RangedEnemyAI>();
    }

    public void TakeDamage(int damage)
    {
        // Chặn sát thương nếu đã chết
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        // TẠO THÔNG TIN THAY ĐỔI MÁU VÀ GỬI CHO THANH MÁU UI CẬP NHẬT
        if (OnHealthChanged != null)
        {
            // GIẢI PHÁP AN TOÀN: Khởi tạo struct mặc định 
            HealthChangeInfo info = new HealthChangeInfo();

            // Mẹo: Nếu bạn muốn biết chính xác các biến bên trong info tên là gì:
            // Bạn chỉ cần gõ "info." rồi nhấn tổ hợp phím Ctrl + Space (Khoảng trắng), 
            // Visual Studio sẽ tự động liệt kê danh sách các biến chính xác đang có sẵn.

            OnHealthChanged.Invoke(info);
        }

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

        DisableEnemyAI();

        _animator?.SetBool("Death", true);

        yield return new WaitForSeconds(0.8f);

        if (_enemyAI != null)
            _enemyAI.Die();
        else if (_rangedEnemyAI != null)
            _rangedEnemyAI.Die();
        else
            Destroy(gameObject);
    }

    private void DisableEnemyAI()
    {
        MonoBehaviour ai = _enemyAI != null ? _enemyAI : (MonoBehaviour)_rangedEnemyAI;
        if (ai == null) return;

        ai.enabled = false;

        if (ai.TryGetComponent(out Rigidbody2D rb))
            rb.linearVelocity = Vector2.zero;
    }
}