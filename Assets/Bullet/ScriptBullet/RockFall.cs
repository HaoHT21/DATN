using UnityEngine;

public class RockFall : MonoBehaviour
{
    [Header("Damage & Knockback")]
    public int damage = 20;
    public float knockbackForce = 8f; // Lực văng Player

    [Header("Collider")]
    public Collider2D hitCollider;
    public GameObject Rock;

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();

        if (hitCollider != null)
            hitCollider.enabled = false;
    }

    // Animation Event tại frame 0.45s
    public void EnableHitbox()
    {
        if (hitCollider != null)
            hitCollider.enabled = true;
    }

    // Animation Event sau khoảng 0.55~0.6s
    public void DisableHitbox()
    {
        if (hitCollider != null)
            hitCollider.enabled = false;
    }

    // Animation Event cuối animation
    public void DestroyRock()
    {
        Destroy(Rock != null ? Rock : gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitCollider.enabled)
            return;

        if (other.CompareTag("Player"))
        {
            // Gây thiệt hại
            if (other.TryGetComponent(out PlayerHealth player))
            {
                player.TakeDamage(damage);
            }

            // Gây Knockback (Hất văng)
            if (other.TryGetComponent(out Rigidbody2D playerRb))
            {
                // Tính hướng hất văng từ tâm viên đá ra ngoài Player
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;

                // Nếu trùng góc hoàn toàn thì hất lên trên nhẹ
                if (knockbackDir == Vector2.zero) knockbackDir = Vector2.up;

                // Reset vận tốc cũ để lực văng chuẩn hơn
                playerRb.linearVelocity = Vector2.zero;
                playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}