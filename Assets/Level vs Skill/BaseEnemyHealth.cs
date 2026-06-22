using System.Collections;
using UnityEngine;

public class BaseEnemyHealth : MonoBehaviour
{
    [Header("--- Thống kê Chỉ số ---")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("--- Phần thưởng khi chết ---")]
    [Tooltip("Số lượng EXP thưởng cho Player khi con quái này nghẻo")]
    public int expReward = 30;
    [Tooltip("Thời gian chờ xóa xác quái vật sau khi chơi xong hoạt ảnh chết")]
    public float destroyDelay = 1f;

    private Animator _animator;
    private EnemyAI _enemyAI;
    private Collider2D _collider;
    private Rigidbody2D _rb;
    private bool _isDead = false;

    private void Awake()
    {
        // Tự động lôi các component trên người con quái ra xài
        _animator = GetComponent<Animator>();
        _enemyAI = GetComponent<EnemyAI>();
        _collider = GetComponent<Collider2D>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Đầu game cho quái đầy cây máu
        currentHealth = maxHealth;
    }

    // --- HÀM NHẬN SÁT THƯƠNG TRỪ MÁU ---
    // (Gọi hàm này từ viên đạn của Player hoặc tia sét gõ J của mày)
    public void TakeDamage(int damage)
    {
        if (_isDead) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} bị vả! Máu còn: {currentHealth}/{maxHealth}");

        // 1. Ép con quái chơi hoạt ảnh giật mình trúng đòn (Hit/Hurt)
        if (_enemyAI != null)
        {
            _enemyAI.PlayHurt(); // Kích hoạt Trigger "Hit" ngoài Animator
        }

        // 2. Kiểm tra nếu cạn máu thì cho nghẻo luôn
        if (currentHealth <= 0)
        {
            ProcessDeath();
        }
    }

    private void ProcessDeath()
    {
        _isDead = true;

        // Khóa kén vật lý để đạn bay xuyên qua xác, không bị kẹt hay tính va chạm nữa
        if (_collider != null) _collider.enabled = false;

        // Dừng lực tịnh tiến Rigidbody ngay lập tức không cho quái lướt bóng ma
        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false;
        }

        // Gọi lệnh sang script AI để nó kích hoạt biến chết chuẩn ngoài Animator
        if (_enemyAI != null)
        {
            _enemyAI.Die();
        }

        // --- CƠ CHẾ THƯỞNG EXP CHO PLAYER ---
        // Tìm thằng Player ngoài Scene theo Tag
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            PlayerHealth playerHealth = playerObj.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Bơm thẳng EXP vào hệ thống thăng cấp của Player
                playerHealth.AddEXP(expReward);
            }
        }

        // Tiến hành xóa sổ con quái khỏi bộ nhớ Scene sau khoảng delay chơi anim chết
        Destroy(gameObject, destroyDelay);
    }
}