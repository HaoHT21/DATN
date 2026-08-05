using UnityEngine;

public class PoisonArea : MonoBehaviour
{
    [Header("Effect Data")]
    public StatusEffectSO poisonEffectSO; // Kéo ScriptableObject Độc vào đây

    [Header("Sát thương trực tiếp")]
    public int directDamage = 5;

    [Header("After Exit")]
    public float poisonDurationAfterExit = 3f;

    [Header("Lifetime")]
    public float lifeTime = 5f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Trừ máu trực tiếp lập tức
            if (other.TryGetComponent<PlayerHealth>(out PlayerHealth player)) // Gây sát thương ngay lập tức cho Player khi va chạm với PoisonArea
            {
                player.TakeDamage(directDamage);
            }

            // 2. Gắn hiệu ứng độc từ ScriptableObject
            ApplyOrRefreshPoison(other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ApplyOrRefreshPoison(other);
        }
    }

    private void ApplyOrRefreshPoison(Collider2D other)
    {
        if (other.TryGetComponent<EffectManager>(out EffectManager effectManager))
        {
            if (poisonEffectSO != null)
            {
                effectManager.ApplyEffect(poisonEffectSO, poisonDurationAfterExit);
            }
        }
    }
}