using UnityEngine;
using System.Collections;

public class EnemyShooter : MonoBehaviour
{
    [Header("Distance")]

    [Tooltip("Khoảng cách đứng bắn")]
    public float attackDistance = 4f;

    [Tooltip("Quá gần thì lùi")]
    public float retreatDistance = 2f;

    [Tooltip("Tốc độ lùi")]
    public float retreatSpeed = 2f;

    [Tooltip("Lùi tối đa bao lâu")]
    public float retreatDuration = 1.5f;


    [Header("Attack")]
    public float fireRate = 2f;
    public float attackDuration = 0.3f;


    [Header("Burst")]
    public int burstCount = 3;
    public float burstInterval = 0.15f;


    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Range(1, 20)]
    public int bulletsPerShot = 3;

    [Range(0, 180)]
    public float spreadAngle = 30f;


    [Header("Visual")]
    public Transform enemyVisual;


    private EnemyController controller;
    private Rigidbody2D rb;

    private bool isAttacking;
    private bool isRetreating;

    private float retreatTimer;
    private float fireTimer;

    private void Awake()
    {
        controller =
            GetComponent<EnemyController>();

        rb =
            GetComponent<Rigidbody2D>();
    }

        
    private void Update()
    {
        fireTimer -= Time.deltaTime;

        if (isAttacking)
            return;

        if (!controller.HasTarget)
            return;

        Transform target =
            controller.Target;

        Vector2 dir =
            target.position -
            transform.position;

        float distance =
            dir.magnitude;

        controller.LookAt(
        target.position);

        // quá gần -> lùi
        if (distance < retreatDistance)
        {
            controller.LockMovement(true);

            Retreat(dir);
            return;
        }

        // đứng bắn
        if (distance <= attackDistance)
        {
            controller.LockMovement(true);

            Attack();
            return;
        }

        // ngoài vùng bắn
        controller.LockMovement(false);

        isRetreating = false;
        retreatTimer = 0;
    }


    void Retreat(Vector2 dir)
    {
        isRetreating = true;

        retreatTimer +=
            Time.deltaTime;

        Vector2 moveDir =
            -dir.normalized;

        rb.linearVelocity =
            moveDir *
            retreatSpeed;

        controller.PlayAnimation(
    "idle");

        // lùi quá lâu vẫn quay lại bắn
        if (retreatTimer >=
            retreatDuration)
        {
            retreatTimer = 0;

            isRetreating = false;

            controller.StopMovement();

            Attack();
        }
    }


    void Attack()
    {
        if (isAttacking)
            return;

        if (fireTimer <= 0)
        {
            fireTimer = fireRate;

            StartCoroutine(
                AttackRoutine());
        }
    }


    IEnumerator AttackRoutine()
    {
        isAttacking = true;

        controller.LockMovement(true);
        controller.StopMovement();

        for (int i = 0; i < burstCount; i++)
        {
            controller.PlayAnimation(
                "attack");

            // đợi hết animation
            yield return new WaitForSeconds(
                attackDuration);

            Shoot();

            if (i < burstCount - 1)
            {
                yield return new WaitForSeconds(
                    burstInterval);
            }
        }

        controller.PlayAnimation("idle");

        controller.LockMovement(false);

        isAttacking = false;
    }


    void Shoot()
    {
        if (
            bulletPrefab == null ||
            firePoint == null
        )
            return;

        float startAngle =
            -spreadAngle * 0.5f;

        float step =
            bulletsPerShot > 1 ?
            spreadAngle /
            (bulletsPerShot - 1)
            : 0;

        for (int i = 0;
            i < bulletsPerShot;
            i++)
        {
            float angle =
                startAngle +
                step * i;

            Quaternion rot =
                firePoint.rotation *
                Quaternion.Euler(
                    0,
                    0,
                    angle);

            Instantiate(
                bulletPrefab,
                firePoint.position,
                rot);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackDistance);

        Gizmos.color =
            Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            retreatDistance);
    }
}