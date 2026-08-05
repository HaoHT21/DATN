using UnityEngine;

public class TrapFire : MonoBehaviour
{
    [Header("Fire Trap Settings")]
    [Tooltip("Tốc độ tích nhiệt mỗi giây khi đứng trong bẫy lửa")]
    public float heatPerSecond = 0.4f;

    [Tooltip("Collider của bẫy lửa (Nếu để trống, script sẽ tự lấy Collider2D trên Object này)")]
    public Collider2D fireTrapCollider;

    private void Start()
    {
        // Tự động tìm Collider2D nếu chưa kéo thả vào Inspector
        if (fireTrapCollider == null)
        {
            fireTrapCollider = GetComponent<Collider2D>();
        }

        // Đảm bảo Collider luôn là Trigger
        if (fireTrapCollider != null)
        {
            fireTrapCollider.isTrigger = true;
        }
    }

    private void ApplyFireHeat(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<EffectManager>(out EffectManager effect))
        {
            effect.AddFireHeat(heatPerSecond * Time.deltaTime);
        }
    }

    // Tích nhiệt ngay frame đầu tiên chạm bẫy
    private void OnTriggerEnter2D(Collider2D other)
    {
        ApplyFireHeat(other);
    }

    // Tích nhiệt liên tục theo từng frame khi đứng trong bẫy
    private void OnTriggerStay2D(Collider2D other)
    {
        ApplyFireHeat(other);
    }
}