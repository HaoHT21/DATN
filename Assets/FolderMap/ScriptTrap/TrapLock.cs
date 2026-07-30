using System.Collections;
using UnityEngine;

public class TrapLock : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    public float lockTime = 2f;
    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
            return;

        if (!other.CompareTag("Player"))
            return;

        activated = true;

        if (animator != null)
            animator.Play("Lock");

        // Gọi EffectManager để khóa cả di chuyển và tấn công
        if (other.TryGetComponent(out EffectManager effect))
        {
            effect.Freeze(lockTime);
        }
    }
}