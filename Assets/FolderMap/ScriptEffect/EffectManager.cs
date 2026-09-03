using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private PlayerController player;
    private PlayerHealth health;

    [Header("Active Effects Runtime")]
    // Danh sách lưu toàn bộ hiệu ứng đang chạy trên Player
    private List<ActiveEffect> activeEffects = new List<ActiveEffect>();

    [Header("Attack Speed")]
    public float baseAttackRate = 0.5f;
    public float currentAttackRate;

    [Header("Base")]
    public float baseMoveSpeed = 5f;

    [Header("Runtime")]
    public float currentMoveSpeed;

    private float speedBonus = 0f;
    // ==========================================
    // QUẢN LÝ CÁC NGUỒN LÀM CHẬM (MULTIPLE SLOW SOURCES)
    // ==========================================
    private List<float> generalSlowSources = new List<float>();
    private List<float> slideSlowSources = new List<float>();

    private int freezeCount = 0;

    public float fireValue;
    public float maxFire = 1f;
    private float lastFireTime;

    // Coroutine lưu trữ riêng cho Poison
    private Coroutine poisonRoutine;

    public bool canAddHeat = true; // Cờ kiểm soát cho phép cộng nhiệt hay không

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        player = GetComponent<PlayerController>();

        currentAttackRate = baseAttackRate;
        ApplyStats();
    }

    private void Update()
    {
        HandleEffectTimers();
    }

    // ==========================================
    // QUẢN LÝ THỜI GIAN VÀ XÓA EFFECT KHI HẾT HẠN
    // ==========================================
    private void HandleEffectTimers()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            ActiveEffect effect = activeEffects[i];
            effect.remainingTime -= Time.deltaTime;

            // Kiểm tra nếu HẾT THỜI GIAN
            if (effect.remainingTime <= 0f)
            {
                RemoveEffect(effect);
            }
        }
    }

    /// <summary>
    /// Hàm chính để gán một Effect vào Player từ SO
    /// </summary>
    public void ApplyEffect(StatusEffectSO effectSO, float customDuration = -1f)
    {
        if (effectSO == null) return;

        float duration = customDuration > 0 ? customDuration : effectSO.baseDuration;

        // 1. Kiểm tra nếu Effect đã tồn tại -> Làm mới lại thời gian (Refresh Duration)
        ActiveEffect existingEffect = activeEffects.Find(e => e.data == effectSO);
        if (existingEffect != null)
        {
            // Reset lại thời gian hiệu ứng nếu dính lại
            existingEffect.remainingTime = duration;
            return;
        }

        // 2. Tạo Visual Prefab trên người Player
        GameObject visualObj = null;
        if (effectSO.visualPrefab != null)
        {
            visualObj = Instantiate(effectSO.visualPrefab, transform.position, Quaternion.identity, transform);
        }

        // 3. Tạo ActiveEffect mới và thêm vào danh sách quản lý
        ActiveEffect newEffect = new ActiveEffect(effectSO, visualObj, duration);
        activeEffects.Add(newEffect);

        // 4. Kích hoạt logic theo từng loại Effect
        OnEffectStarted(newEffect);
    }

    /// <summary>
    /// Xóa Effect khi hết thời gian hoặc bị xóa chủ động
    /// </summary>
    public void RemoveEffect(ActiveEffect effect)
    {
        if (effect == null || !activeEffects.Contains(effect)) return;

        // 1. Xóa Visual Prefab trên người Player
        if (effect.visualInstance != null)
        {
            Destroy(effect.visualInstance);
        }

        // 2. Tắt logic chỉ số / trạng thái
        OnEffectEnded(effect);

        // 3. Rút khỏi danh sách quản lý
        activeEffects.Remove(effect);
    }

    // Hàm xóa Effect theo SO Data (cho IceZone dùng khi Player đi ra ngoài)
    public void RemoveEffectBySO(StatusEffectSO effectSO)
    {
        ActiveEffect effect = activeEffects.Find(e => e.data == effectSO);
        if (effect != null)
        {
            RemoveEffect(effect);
        }
    }

    // ==========================================
    // BẮT ĐẦU & KẾT THÚC LOGIC CHO TỪNG LOẠI EFFECT
    // ==========================================
    private void OnEffectStarted(ActiveEffect effect)
    {
        switch (effect.data.effectType)
        {
            case EffectType.Freeze:
                AddFreeze();
                break;
            case EffectType.Slow:
                AddSlow(0.5f); // Ví dụ slow 50%
                break;
            case EffectType.Burn:
                // Burn sẽ được xử lý trong FireZone.cs, nên không cần logic ở đây
                break;
            case EffectType.Poison:
                // Kích hoạt Coroutine rút máu theo interval
                if (poisonRoutine != null) StopCoroutine(poisonRoutine);
                poisonRoutine = StartCoroutine(PoisonRoutine(2, 1f)); // Mặc định: 2 dame mỗi 1s
                break;
            case EffectType.Shield:
                // Bật bất tử khi nhận Effect Khiên
                if (health != null)
                {
                    health.SetInvincible(true);
                }
                break;
        }
    }

    private void OnEffectEnded(ActiveEffect effect)
    {
        switch (effect.data.effectType)
        {
            case EffectType.Freeze:
                RemoveFreeze();
                break;
            case EffectType.Slow:
                RemoveSlow(0.5f); // Ví dụ slow 50%
                break;
            case EffectType.Poison:
                if (poisonRoutine != null)
                {
                    StopCoroutine(poisonRoutine);
                    poisonRoutine = null;
                }
                break;
            case EffectType.Shield:
                // Tắt bất tử khi hết thời gian Effect Khiên
                if (health != null)
                {
                    health.SetInvincible(false);
                }
                break;
        }
    }

    private IEnumerator PoisonRoutine(int damage, float interval)
    {
        while (true)
        {
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            yield return new WaitForSeconds(interval);
        }
    }

    private void ApplyStats()
    {
        // 1. Xử lý trạng thái Đóng băng (Freeze)
        if (freezeCount > 0)
        {
            currentMoveSpeed = 0f;
            player.moveSpeed = 0f;

            // Bật trạng thái bị đóng băng ở PlayerController
            if (player != null)
                player.isFrozen = true;
        }
        else
        {
            if (player != null)
                player.isFrozen = false;

            float speed = baseMoveSpeed + speedBonus;

            // Lấy ra tỉ lệ Slow mạnh nhất từ danh sách General Slow
            float maxSlow = 0f;
            foreach (float slow in generalSlowSources)
            {
                if (slow > maxSlow) maxSlow = slow;
            }
            speed *= (1f - maxSlow);

            // Lấy ra tỉ lệ Slow mạnh nhất từ danh sách Slide Slow
            float maxSlideSlow = 0f;
            foreach (float slow in slideSlowSources)
            {
                if (slow > maxSlideSlow) maxSlideSlow = slow;
            }
            speed *= (1f - maxSlideSlow);

            currentMoveSpeed = speed;
            if (player != null)
            {
                player.moveSpeed = speed;
                player.attackRate = currentAttackRate;
            }
        }
    }

    //=========================
    // Speed
    //=========================
    public void AddSpeed(float amount)
    {
        speedBonus += amount;
        ApplyStats();
    }

    public void RemoveSpeed(float amount)
    {
        speedBonus -= amount;
        ApplyStats();
    }

    public void AddSpeedTemporary(float amount, float duration)
    {
        StartCoroutine(SpeedRoutine(amount, duration));
    }

    IEnumerator SpeedRoutine(float amount, float duration)
    {
        AddSpeed(amount);
        yield return new WaitForSeconds(duration);
        RemoveSpeed(amount);
    }

    //=========================
    // General Slow (Tích lũy theo danh sách)
    //=========================
    public void AddSlow(float percent)
    {
        generalSlowSources.Add(percent);
        ApplyStats();
    }

    public void RemoveSlow(float percent)
    {
        generalSlowSources.Remove(percent);
        ApplyStats();
    }

    //=========================
    // Slide Slow (Tích lũy theo danh sách)
    //=========================
    public void AddSlideSlow(float percent)
    {
        slideSlowSources.Add(percent);
        ApplyStats();
    }

    public void RemoveSlideSlow(float percent)
    {
        slideSlowSources.Remove(percent);
        ApplyStats();
    }

    //=========================
    // Freeze (Đóng băng - Khóa di chuyển + Khóa tấn công)
    //=========================
    public void AddFreeze()
    {
        freezeCount++;
        ApplyStats();
    }

    public void RemoveFreeze()
    {
        freezeCount--;
        if (freezeCount < 0)
            freezeCount = 0;

        ApplyStats();
    }

    public void Freeze(float duration)
    {
        StartCoroutine(FreezeRoutine(duration));
    }

    IEnumerator FreezeRoutine(float duration)
    {
        AddFreeze();
        yield return new WaitForSeconds(duration);
        RemoveFreeze();
    }

    //=========================
    // Attack Speed
    //=========================
    public void AddAttackSpeed(float percent)
    {
        currentAttackRate *= 1f - percent;

        if (currentAttackRate < 0.05f)
            currentAttackRate = 0.05f;

        ApplyStats();
    }

    public void RemoveAttackSpeed(float percent)
    {
        currentAttackRate /= 1f - percent;
        ApplyStats();
    }

    public void AddAttackSpeedTemporary(float percent, float duration)
    {
        StartCoroutine(AttackSpeedRoutine(percent, duration));
    }

    IEnumerator AttackSpeedRoutine(float percent, float duration)
    {
        float original = baseAttackRate;

        currentAttackRate = Mathf.Max(
            original * (1f - percent),
            0.05f);

        ApplyStats();

        yield return new WaitForSeconds(duration);

        currentAttackRate = original;

        ApplyStats();
    }

    // Xử lý Nhiệt Lửa (Fire Heat)
    public void ResetFireHeat()
    {
        fireValue = 0;
    }

    public void AddFireHeat(float amount)
    {
        // Nếu bị khóa (đang trong quá trình xả nhiệt/đốt máu) thì KHÔNG nhận thêm nhiệt
        if (!canAddHeat)
            return;

        fireValue += amount;
        fireValue = Mathf.Clamp(fireValue, 0, maxFire);
        lastFireTime = Time.time;
    }

    public float LastFireTime => lastFireTime;
}