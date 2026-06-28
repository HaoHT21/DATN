using UnityEngine;

public class BulletDamage : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 15;

    [Header("Move")]
    public float speed = 8f;

    [Header("Life Time")]
    public float lifeTime = 5f;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Bay thẳng theo hướng Right của FirePoint
        rb.linearVelocity = transform.right * speed;

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Trúng Player
        if (other.CompareTag("Player"))
        {
            Debug.Log("Bullet Hit Player");

            PlayerHealth player =
                other.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage);
            }

            Destroy(gameObject);
            return;
        }

        // Trúng tường thì hủy
        if (other.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}