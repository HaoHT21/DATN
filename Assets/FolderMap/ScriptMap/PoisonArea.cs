using UnityEngine;

public class PoisonArea : MonoBehaviour
{
    [Header("Poison")]
    public int poisonDamage = 1;
    public float damageInterval = 1f;

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
        if (other.TryGetComponent<PlayerHealth>(out PlayerHealth player))
        {
            PoisonEffect poison =
                player.GetComponent<PoisonEffect>();

            if (poison == null)
                poison = player.gameObject.AddComponent<PoisonEffect>();

            poison.ApplyPoison(
                poisonDamage,
                damageInterval,
                poisonDurationAfterExit);
        }
    }
}