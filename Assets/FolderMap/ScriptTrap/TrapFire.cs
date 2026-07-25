using UnityEngine;

public class TrapFire : MonoBehaviour
{
    public float heatPerSecond = .4f;

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        EffectManager effect =
            other.GetComponent<EffectManager>();

        if (effect != null)
        {
            effect.AddFireHeat(
                heatPerSecond *
                Time.deltaTime);
        }
    }
}