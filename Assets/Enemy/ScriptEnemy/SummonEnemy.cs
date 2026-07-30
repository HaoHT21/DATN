using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SummonEnemy : MonoBehaviour
{
    public float moveSpeed = 3f;

    public float detectRange = 15f;

    public float attackRange = 5f;

    public float attackRate = 0.5f;

    public float lifeTime = 10f;

    public Transform firePoint;

    public GameObject bulletPrefab;

    Rigidbody2D rb;

    Transform target;

    float attackTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        FindTarget();

        if (target == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float distance =
            Vector2.Distance(
                transform.position,
                target.position);

        if (distance > attackRange)
        {
            Vector2 dir =
                (target.position -
                transform.position).normalized;

            rb.linearVelocity =
                dir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;

            attackTimer -= Time.deltaTime;

            if (attackTimer <= 0)
            {
                attackTimer = attackRate;

                Shoot();
            }
        }
    }

    void FindTarget()
    {
        target = null;

        float best = detectRange;

        EnemyHeath[] enemies =
            FindObjectsByType<EnemyHeath>(
                FindObjectsSortMode.None);

        foreach (EnemyHeath e in enemies)
        {
            if (e == null)
                continue;

            float d =
                Vector2.Distance(
                    transform.position,
                    e.transform.position);

            if (d < best)
            {
                best = d;
                target = e.transform;
            }
        }

        if (target != null)
            return;

        BossHeath[] bosses =
            FindObjectsByType<BossHeath>(
                FindObjectsSortMode.None);

        foreach (BossHeath b in bosses)
        {
            float d =
                Vector2.Distance(
                    transform.position,
                    b.transform.position);

            if (d < best)
            {
                best = d;
                target = b.transform;
            }
        }
    }

    void Shoot()
    {
        if (bulletPrefab == null)
            return;

        Vector2 dir =
            (target.position -
            firePoint.position).normalized;

        float angle =
            Mathf.Atan2(
                dir.y,
                dir.x) *
            Mathf.Rad2Deg;

        Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.Euler(
                0,
                0,
                angle));
    }
}