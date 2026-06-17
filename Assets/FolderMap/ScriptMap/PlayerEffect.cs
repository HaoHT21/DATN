using System.Collections;
using UnityEngine;

public class PlayerEffect : MonoBehaviour
{
    private PlayerController player;

    [Header("Chỉ số gốc")]
    public float baseMoveSpeed = 5f;

    [Header("Chỉ số hiện tại")]
    public float currentMoveSpeed;

    private void Awake()
    {
        player = GetComponent<PlayerController>();

        currentMoveSpeed = baseMoveSpeed;
        ApplyStats();
    }

    void ApplyStats()
    {
        player.moveSpeed = currentMoveSpeed;
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
}