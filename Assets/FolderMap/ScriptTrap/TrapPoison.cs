using System.Collections;
using UnityEngine;

public class TrapPoison1 : MonoBehaviour
{
    [Header("Effect Data")]
    public StatusEffectSO poisonEffectSO; // Kéo ScriptableObject Độc vào đây

    [Header("Trap Setting")]
    [Tooltip("Kéo Collider2D của bẫy độc vào đây. Nếu để trống, script sẽ tự lấy Collider2D trên Object này.")]
    public Collider2D trapPoisonCollider;

    [Header("Hiệu ứng sau khi rời bẫy")]
    [Tooltip("Thời gian độc tồn tại trên người sau khi giẫm vào bẫy")]
    public float poisonDurationAfterExit = 3f;

    private void Start()
    {
        // Nếu không kéo thủ công vào Inspector, tự động tìm Collider2D trên GameObject này
        if (trapPoisonCollider == null)
        {
            trapPoisonCollider = GetComponent<Collider2D>();
        }

        // Tự động đảm bảo Collider luôn là Trigger
        if (trapPoisonCollider != null)
        {
            trapPoisonCollider.isTrigger = true;
        }
    }

    private void HandlePoison(Collider2D other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent<EffectManager>(out EffectManager effectManager))
        {
            if (poisonEffectSO != null)
            {
                // Áp dụng status effect độc và truyền thời gian tác dụng
                effectManager.ApplyEffect(poisonEffectSO, poisonDurationAfterExit);
            }
        }
    }

    // Dính độc ngay frame đầu tiên bước vào
    private void OnTriggerEnter2D(Collider2D other)
    {
        HandlePoison(other);
    }

    // Liên tục làm mới thời gian độc nếu tiếp tục đứng trong bẫy
    private void OnTriggerStay2D(Collider2D other)
    {
        HandlePoison(other);
    }
}