using System.Collections;
using UnityEngine;

public class PlayerEffect : MonoBehaviour
{
    private PlayerController player;

    [Header("Chỉ số gốc")]
    public float baseMoveSpeed = 5f;

    [Header("Chỉ số hiện tại")]
    public float currentMoveSpeed;

    private int freezeCount = 0;

    private float slowMultiplier = 1f;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        currentMoveSpeed = baseMoveSpeed;
        ApplyStats();
    }

    void ApplyStats()
    {
        float speed = baseMoveSpeed * slowMultiplier;

        if (freezeCount > 0)
            speed = 0;

        currentMoveSpeed = speed;
        player.moveSpeed = speed;
    }

    // Tăng tốc vĩnh viễn
    public void AddSpeed(float amount)
    {
        currentMoveSpeed += amount;
        ApplyStats();
    }

    // Giảm tốc
    public void RemoveSpeed(float amount)
    {
        currentMoveSpeed -= amount;
        ApplyStats();
    }

    public void AddFreeze()
    {
        freezeCount++;

        currentMoveSpeed = 0;
        ApplyStats();
    }

    public void RemoveFreeze()
    {
        freezeCount--;

        if (freezeCount < 0)
            freezeCount = 0;

        if (freezeCount == 0)
        {
            currentMoveSpeed = baseMoveSpeed;
            ApplyStats();
        }
    }

    // Buff có thời gian
    public void AddSpeedTemporary(float amount, float duration)
    {
        StartCoroutine(SpeedBuffRoutine(amount, duration));
    }

    IEnumerator SpeedBuffRoutine(float amount, float duration)
    {
        currentMoveSpeed += amount;
        ApplyStats();

        yield return new WaitForSeconds(duration);

        currentMoveSpeed -= amount;
        ApplyStats();
    }

    public void Freeze(float duration)
    {
        AddFreeze();
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveFreeze();
    }

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
}