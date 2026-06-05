using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // BẮT BUỘC: Thêm thư viện này để sử dụng Slider

public class PlayerHealth : MonoBehaviour, IHealthProvider
{
    [Header("--- UI Elements ---")]
    public Slider healthSlider; // Ô kéo thả thanh máu trong Inspector

    [Header("--- Health Settings ---")]
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

    private void Start()
    {
        // Cập nhật thanh máu chuẩn ngay khi vào game
        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        int before = currentHealth;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        NotifyHealthChanged(before);
        UpdateHealthUI(); // Cập nhật UI sau khi mất máu

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
        UpdateHealthUI(); // Cập nhật UI sau khi được hồi máu
    }

    private void NotifyHealthChanged(int previousHealth)
    {
        OnHealthChanged?.Invoke(new HealthChangeInfo(currentHealth, maxHealth, currentHealth - previousHealth));
    }

    // --- HÀM CẬP NHẬT UI THANH MÁU ---
    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            // Ép kiểu float để phép chia chính xác (ví dụ: 50 / 100 = 0.5f)
            healthSlider.value = (float)currentHealth / maxHealth;
        }
        else
        {
            // Dự phòng nếu bạn quên kéo thả Slider vào ô Inspector, tự động tìm theo tên
            GameObject hpObj = GameObject.Find("HealthSlider");
            if (hpObj != null)
            {
                healthSlider = hpObj.GetComponent<Slider>();
                healthSlider.value = (float)currentHealth / maxHealth;
            }
        }
    }

    private void PlayerDie()
    {
        IsDead = true;

        _animator.SetBool("Dead", true);

        // Giữ nguyên linearVelocity theo Unity bản mới của bạn
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
        UpdateHealthUI(); // Cập nhật lại UI đầy cây máu sau khi hồi sinh

        _rb.simulated = true;

        _animator.SetBool("Dead", false);

        Debug.Log("ĐÃ HỒI SINH TẠI SẢNH!");
    }
}