using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; // BẮT BUỘC: Thêm thư viện này để sử dụng Slider
using TMPro; // BẮT BUỘC: Thêm thư viện này để sử dụng TextMeshProUGUI

public class PlayerHealth : MonoBehaviour, IHealthProvider
{
    [Header("--- UI Elements ---")]
    [Tooltip("Kéo thả thanh HP_Slider ngoài Canvas vào đây")]
    public Slider healthSlider;
    [Tooltip("Kéo thả thanh MP_Slider ngoài Canvas vào đây")]
    public Slider manaSlider;
    [Tooltip("Kéo thả cái Object Level_Text ngoài Canvas vào đây")]
    public TextMeshProUGUI levelText; // Biến UI hiển thị chữ Level

    [Header("--- UI Text Numbers ---")]
    [Tooltip("Kéo thả Object HP_Text (chữ hiển thị số máu) vào đây")]
    public TextMeshProUGUI hpText;
    [Tooltip("Kéo thả Object MP_Text (chữ hiển thị số mana) vào đây")]
    public TextMeshProUGUI mpText;

    [Header("--- Level & EXP Settings ---")]
    public int currentLevel = 1;
    [Tooltip("Player 1 điền số 5, các Player khác điền số 8 ngoài Inspector")]
    public int maxLevel = 8;
    public int currentEXP = 0;

    // MỞ RỘNG MẢNG EXP ĐẾN CẤP 8 THEO YÊU CẦU ĐỂ TRÁNH LỖI VĂNG MẢNG:
    // Cấp 1 cần 100, Cấp 2 cần 250, Cấp 3 cần 500, Cấp 4 cần 1000 để lên Cấp 5
    // Cấp 5 cần 1500, Cấp 6 cần 2200, Cấp 7 cần 3000 để lên Cấp 8 tối đa
    public int[] expToNextLevel = new int[] { 100, 250, 500, 1000, 1500, 2200, 3000 };

    [Header("--- Health Settings ---")]
    public int currentHealth = 100;
    public int maxHealth = 100;
    public bool IsDead { get; private set; }

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    [Header("--- Mana Settings ---")]
    public int currentMana = 100;
    public int maxMana = 100;

    [Header("--- Mana Regen Settings ---")]
    [Tooltip("Số lượng Mana tự động hồi phục sau mỗi 1 giây")]
    public float manaRegenRate = 5f;
    private float manaRegenTimer;

    public event Action<HealthChangeInfo> OnHealthChanged;

    [Header("Respawn Settings")]
    public Vector3 spawnPosition; // Vị trí điểm hồi sinh ở Sảnh

    private bool isInBadEndZone = false;
    private GameObject badEndUI;

    private Animator _animator;
    private Rigidbody2D _rb;
    private PlayerController _playerController; // Khóa súng ống khi chết để tránh lỗi dính đạn

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        _playerController = GetComponent<PlayerController>();
        PlayerEffect effect = GetComponent<PlayerEffect>();

        if (effect != null)
        {
            effect.ResetEffects();
        }

        // Khởi tạo đầy cây máu và cây mana khi vào game
        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    private void Start()
    {
        // Cập nhật giao diện UI góc trái ngay khi vừa vào game
        UpdateUI();
    }

    public void SetInBadEndZone(bool state, GameObject uiCanvas)
    {
        isInBadEndZone = state;
        badEndUI = uiCanvas;
    }

    private void Update()
    {
        if (IsDead) return;

        // --- CƠ CHẾ TỰ ĐỘNG HỒI MANA THEO THỜI GIAN THỰC ---
        if (currentMana < maxMana)
        {
            manaRegenTimer += Time.deltaTime * manaRegenRate;
            if (manaRegenTimer >= 1f)
            {
                int amountToRegen = Mathf.FloorToInt(manaRegenTimer);
                manaRegenTimer -= amountToRegen;

                currentMana += amountToRegen;
                if (currentMana > maxMana) currentMana = maxMana;

                UpdateUI(); // Đồng bộ ngay lập tức lên thanh MP_Slider ngoài màn hình
            }
        }
    }

    // --- LOGIC QUẢN LÝ CẤP ĐỘ (LEVEL) VÀ KINH NGHIỆM (EXP) ---
    public void AddEXP(int amount)
    {
        // Nếu chạm mốc maxLevel của riêng con đó hoặc đã chết thì ngắt luôn, đéo nhận thêm EXP
        if (IsDead || currentLevel >= maxLevel) return;

        currentEXP += amount;

        // ĐÃ FIX: Thêm điều kiện (currentLevel - 1 < expToNextLevel.Length) để bảo vệ mảng tuyệt đối ngoài Editor
        while (currentLevel < maxLevel
               && (currentLevel - 1) < expToNextLevel.Length
               && currentEXP >= expToNextLevel[currentLevel - 1])
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        // Khấu trừ lượng EXP vừa sử dụng để thăng cấp
        currentEXP -= expToNextLevel[currentLevel - 1];
        currentLevel++;

        // PHẦN THƯỞNG CHỈ SỐ: Tăng giới hạn tối đa của HP và MP
        maxHealth += 20; // Cứ lên 1 cấp tăng 20 Máu tối đa
        maxMana += 10;   // Cứ lên 1 cấp tăng 10 Mana tối đa

        // Thưởng thêm: Hồi phục đầy trạng thái ngay khi thăng cấp
        currentHealth = maxHealth;
        currentMana = maxMana;

        UpdateUI(); // Đồng bộ lại tỉ lệ thanh Slider mới lên giao diện góc trái

        // CHÈN ĐOẠN HIỆN TEXT CHÚC MỪNG LÊN GIAO DIỆN GAME:
        if (SkillNotification.Instance != null)
        {
            SkillNotification.Instance.ShowMessage($"LEVEL UP! ĐẠT CẤP {currentLevel}", Color.green);

            // Tự động phân tích mốc mở khóa chiêu để nhắc nhở người chơi sau 1.2 giây
            if (currentLevel == 4)
            {
                StartCoroutine(ShowUnlockNoticeDelayed("ĐÃ MỞ KHÓA SKILL MỚI (PHÍM L)!", Color.yellow));
            }
            else if (currentLevel == 7)
            {
                StartCoroutine(ShowUnlockNoticeDelayed("TUYỆT CHIÊU CUỐI ĐÃ SẴN SÀNG (PHÍM M)!", Color.cyan));
            }
        }

        Debug.LogWarning($"CHÚC MỪNG ĐẠI CA! BẠN ĐÃ LÊN CẤP {currentLevel}!!! MaxHP: {maxHealth} | MaxMP: {maxMana}");
    }

    // Hàm bổ trợ gọi thông báo trễ giúp các hiệu ứng text không bị chèn lên nhau gạt mắt
    private IEnumerator ShowUnlockNoticeDelayed(string message, Color textColor)
    {
        yield return new WaitForSeconds(1.2f);
        if (SkillNotification.Instance != null && !IsDead)
        {
            SkillNotification.Instance.ShowMessage(message, textColor);
        }
    }

    // --- LOGIC QUẢN LÝ MÁU (HP) ---
    public void TakeDamage(int damage)
    {
        if (IsDead) return;

        // --- ĐỒNG BỘ MỚI: Gọi con khiên ra hấp thụ damage bằng ref ---
        PlayerShield shield = GetComponent<PlayerShield>();
        if (shield != null)
        {
            shield.AbsorbDamage(ref damage);
        }

        // Nếu khiên đã nuốt hết sát thương (damage giảm về 0), ngắt luôn đéo trừ máu gốc!
        if (damage <= 0) return;

        int before = currentHealth;
        currentHealth -= damage;
        if (currentHealth < 0) currentHealth = 0;

        NotifyHealthChanged(before);
        UpdateUI(); // Cập nhật UI tụt máu khít khịt theo viền pixel

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
        UpdateUI(); // Cập nhật UI tăng máu
    }

    // --- LOGIC QUẢN LÝ MANA (MP) ---
    public bool UseMana(int amount)
    {
        if (IsDead) return false;

        if (currentMana >= amount)
        {
            currentMana -= amount;
            UpdateUI(); // Cập nhật tụt mana ngoài màn hình UI
            return true; // Trả về Đủ Mana để kích hoạt skill thành công
        }

        Debug.LogWarning("ĐÉO ĐỦ MANA RỒI ĐẠI CA ĐÓNG SKILL SAO ĐƯỢC!");
        return false; // Không đủ mana để kích hoạt chiêu
    }

    public void ReplenishMana(int amount)
    {
        if (IsDead) return;
        currentMana += amount;
        if (currentMana > maxMana) currentMana = maxMana;
        UpdateUI();
    }

    private void NotifyHealthChanged(int previousHealth)
    {
        OnHealthChanged?.Invoke(new HealthChangeInfo(currentHealth, maxHealth, currentHealth - previousHealth));
    }

    // --- HÀM ĐỒNG BỘ CẬP NHẬT UI GÓC TRÁI MÀN HÌNH ---
    private void UpdateUI()
    {
        // 1. Đồng bộ thanh Máu (HP Slider + Text số)
        if (healthSlider != null)
        {
            healthSlider.value = (float)currentHealth / maxHealth;
        }
        else
        {
            GameObject hpObj = GameObject.Find("HP_Slider");
            if (hpObj != null)
            {
                healthSlider = hpObj.GetComponent<Slider>();
                healthSlider.value = (float)currentHealth / maxHealth;
            }
        }

        // Cập nhật text số cho Máu (Ví dụ: 100/100)
        if (hpText != null)
        {
            hpText.text = currentHealth + "/" + maxHealth;
        }

        // 2. Đồng bộ thanh Mana (MP Slider + Text số)
        if (manaSlider != null)
        {
            manaSlider.value = (float)currentMana / maxMana;
        }
        else
        {
            GameObject mpObj = GameObject.Find("MP_Slider");
            if (mpObj != null)
            {
                manaSlider = mpObj.GetComponent<Slider>();
                manaSlider.value = (float)currentMana / maxMana;
            }
        }

        // Cập nhật text số cho Mana (Ví dụ: 85/100)
        if (mpText != null)
        {
            mpText.text = currentMana + "/" + maxMana;
        }

        // 3. Đồng bộ hiển thị chữ Level (TMP) lên góc trái
        if (levelText != null)
        {
            levelText.text = "LV. " + currentLevel;
        }
        else
        {
            GameObject lvlObj = GameObject.Find("Level_Text");
            if (lvlObj != null)
            {
                levelText = lvlObj.GetComponent<TextMeshProUGUI>();
                levelText.text = "LV. " + currentLevel;
            }
        }
    }

    // --- LOGIC XỬ LÝ KHI PLAYER NGHẺO VÀ HỒI SINH TỰ ĐỘNG ---
    private void PlayerDie()
    {
        IsDead = true;

        PlayerEffect effect = GetComponent<PlayerEffect>();
        if (effect != null)
        {
            effect.ResetEffects();
        }

        if (_animator != null) _animator.SetBool("Dead", true);

        // Khóa chết script điều khiển súng và giấu cây súng đi để xóa sạch rác lỗi Null lúc nằm xuống
        if (_playerController != null)
        {
            _playerController.enabled = false;
            if (_playerController.weaponHolder != null)
            {
                _playerController.weaponHolder.gameObject.SetActive(false);
            }
        }

        if (_rb != null)
        {
            _rb.linearVelocity = Vector2.zero;
            _rb.simulated = false;
        }

        if (isInBadEndZone && badEndUI != null)
        {
            Debug.Log("<color=red>[BAD ENDING]</color> Player chết tại khu vực Boss! Chặn đứng chuỗi hồi sinh.");

            badEndUI.SetActive(true); // Bật bảng Bad Ending Canvas lên màn hình
            Time.timeScale = 0f;      // Ngừng thời gian của hệ thống game

            return; // CHẶN ĐỨNG: Thoát hàm sớm, không cho phép chạy Coroutine hồi sinh bên dưới
        }

        Debug.Log("PLAYER ĐÃ CHẾT! Đang chờ hồi sinh...");
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(3f); // Chờ 3 giây chơi animation chết xong hồi sinh

        transform.position = spawnPosition;

        int before = currentHealth;
        currentHealth = maxHealth;
        currentMana = maxMana; // Hồi sinh dậy đầy bình cả Máu lẫn Mana cực đẹp theo mốc level hiện tại
        IsDead = false;

        NotifyHealthChanged(before);
        UpdateUI();

        if (_rb != null) _rb.simulated = true;
        if (_animator != null) _animator.SetBool("Dead", false);

        // Kích hoạt mở khóa súng ống và hiện lại cây súng phẳng lì cho player chiến đấu tiếp
        if (_playerController != null)
        {
            _playerController.enabled = true;
            if (_playerController.weaponHolder != null)
            {
                _playerController.weaponHolder.gameObject.SetActive(true);
            }
        }

        Debug.Log("ĐÃ HỒI SINH TẠI SẢNH THÀNH CÔNG!");
    }
}