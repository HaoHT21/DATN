using System.Collections;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    private PlayerController player;

    [Header("Attack Speed")]
    public float baseAttackRate = 0.5f;
    public float currentAttackRate;

    [Header("Base")]
    public float baseMoveSpeed = 5f;

    [Header("Runtime")]
    public float currentMoveSpeed;

    private float speedBonus = 0f;
    private float slowMultiplier = 1f;
    private float slideSlowMultiplier = 1f;

    private int freezeCount = 0;

    public float fireValue;
    public float maxFire = 1f;
    private float lastFireTime;

    private PlayerHealth health;

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        player = GetComponent<PlayerController>();

        currentAttackRate = baseAttackRate;
        ApplyStats();
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
            // Tắt trạng thái đóng băng
            if (player != null)
                player.isFrozen = false;

            // 2. Tính toán lại Tốc độ di chuyển khi hết Freeze
            float speed = baseMoveSpeed + speedBonus;
            speed *= slowMultiplier;
            speed *= slideSlowMultiplier;

            currentMoveSpeed = speed;
            player.moveSpeed = speed;
            player.attackRate = currentAttackRate; // Giữ nguyên attackRate chuẩn
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
    // Slow
    //=========================
    public void SetSlow(float percent)
    {
        slowMultiplier = 1f - percent;
        ApplyStats();
    }

    public void RemoveSlow()
    {
        slowMultiplier = 1f;
        ApplyStats();
    }

    public void SetSlideSlow(float percent)
    {
        slideSlowMultiplier = 1f - percent;
        ApplyStats();
    }

    public void RemoveSlideSlow()
    {
        slideSlowMultiplier = 1f;
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

    public void ResetFireHeat()
    {
        fireValue = 0;
    }

    public void AddFireHeat(float amount)
    {
        fireValue += amount;
        fireValue = Mathf.Clamp(fireValue, 0, maxFire);
        lastFireTime = Time.time;
    }

    public float LastFireTime => lastFireTime;
}