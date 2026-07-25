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

        ApplyStats();

        currentAttackRate = baseAttackRate;
        player.attackRate = currentAttackRate;
    }

    private void ApplyStats()
    {
        float speed = baseMoveSpeed + speedBonus;

        speed *= slowMultiplier;
        speed *= slideSlowMultiplier;

        if (freezeCount > 0)
            speed = 0;

        currentMoveSpeed = speed;
        player.moveSpeed = speed;
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

    //=========================

    // Temporary Buff

    //=========================

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

    //=========================
    // Slide Slow
    //=========================

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

    // Freeze

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

    public void AddAttackSpeed(float percent)
    {
        currentAttackRate *= 1f - percent;

        if (currentAttackRate < 0.05f)
            currentAttackRate = 0.05f;

        player.attackRate = currentAttackRate;

    }

    public void RemoveAttackSpeed(float percent)
    {
        currentAttackRate /= 1f - percent;

        player.attackRate = currentAttackRate;
    }

    public void AddAttackSpeedTemporary(
    float percent,
    float duration)
    {
        StartCoroutine(
            AttackSpeedRoutine(percent, duration));
    }



    IEnumerator AttackSpeedRoutine(
    float percent,
    float duration)
    {
        float old = player.attackRate;

        player.attackRate *= 1f - percent;

        yield return new WaitForSeconds(duration);

        player.attackRate = old;
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