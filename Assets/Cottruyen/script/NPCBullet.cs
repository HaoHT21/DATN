using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class NPCBullet : MonoBehaviour
{
    public float speed = 8f;

    // Bỏ gán cứng "= 10" đi, để nhận giá trị động từ NPCCombat truyền qua
    [HideInInspector] public int damage;

    private Rigidbody2D _rb;
    public GameObject hitEffectPrefab;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Start()
    {
        if (_rb != null)
        {
            _rb.linearVelocity = transform.right * speed;
        }
        Destroy(gameObject, 3f);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy") || col.GetComponent<NPCHealth>() || col.GetComponent<EnemyHealth>())
        {
            return;
        }

        if (col.isTrigger && !col.CompareTag("Player"))
        {
            return;
        }

        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1f);
        }

        if (col.CompareTag("Player"))
        {
            PlayerHealth playerHealth = col.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Trừ máu Player bằng lượng sát thương cụ thể của chiêu thức đó
                playerHealth.TakeDamage(damage);
                Debug.Log($"<color=red>💥 [NPC Bullet]</color> Gây thành công <color=yellow>{damage}</color> sát thương lên Player.");
            }
        }

        Destroy(gameObject);
    }
}