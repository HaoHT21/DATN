using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class IceBullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;

    [Header("Freeze")]
    public float freezeDuration = 2f; //Thời gian đóng băn

    [Header("Effect")]
    public GameObject hitEffectPrefab;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        GetComponent<Collider2D>().isTrigger = true;

        rb.linearVelocity =
            transform.right * speed;

        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.isTrigger)
            return;

        // Player
        if (col.TryGetComponent(out PlayerHealth player))
        {
            player.TakeDamage(damage);

            if (col.TryGetComponent(out PlayerEffect effect))
            {
                effect.Freeze(freezeDuration);
            }

            Hit();
            return;
        }
    }

    private void Hit()
    {
        if (hitEffectPrefab != null)
        {
            Instantiate(
                hitEffectPrefab,
                transform.position,
                Quaternion.identity);
        }

        Destroy(gameObject);
    }
}