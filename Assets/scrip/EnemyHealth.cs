using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour, IHealthProvider
{
    public int currentHealth = 100;
    public int maxHealth = 100; // Cần có maxHealth để UI tính tỷ lệ % thanh máu đầy/vơi

    private Animator _animator;
    private Collider2D _collider;
    private EnemyAI _enemyAI; // Thêm biến này để gọi hàm Die()

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
        _enemyAI = GetComponent<EnemyAI>(); // Lấy script EnemyAI
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

        // Dừng mọi chuyển động của kẻ địch
        if (_enemyAI != null)
        {
            _enemyAI.enabled = false; // Tắt AI để nó không đuổi theo nữa

            // Kiểm tra xem Rigidbody2D có tồn tại hay không trước khi gán vận tốc
            Rigidbody2D rb = _enemyAI.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
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