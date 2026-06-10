using UnityEngine;

public class FireDamageZone : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 10;

    [Header("Damage Interval")]
    public float damageInterval = 0.5f;

    private float damageTimer;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        damageTimer += Time.deltaTime;

        if (damageTimer >= damageInterval)
        {
            damageTimer = 0f;

            PlayerHealth playerHealth =
                other.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            damageTimer = 0f;
        }
    }
}