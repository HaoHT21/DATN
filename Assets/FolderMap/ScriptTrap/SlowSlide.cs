using UnityEngine;

public class SlowSlide : MonoBehaviour
{
    [Header("Slow")]
    [Range(0f, 1f)]
    public float slowPercent = 0.5f;

    private EffectManager effect;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        effect = other.GetComponent<EffectManager>();

        if (effect != null)
            effect.AddSlideSlow(slowPercent);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (effect != null)
            effect.RemoveSlideSlow(slowPercent);
    }
}