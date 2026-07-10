using UnityEngine;

public class RockFall : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 20;

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
        Destroy(Rock);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hitCollider.enabled)
            return;

        if (other.TryGetComponent(out PlayerHealth player))
        {
            player.TakeDamage(damage);
        }
    }
}