using UnityEngine;
using System.Collections;

public class EnemyCircleShoot : MonoBehaviour
{
    [Header("Distance")]

    [Header("Vision")]
    public LayerMask wallLayer;

    [Tooltip("Khoảng cách đứng bắn")]
    public float attackDistance = 4f;

    [Tooltip("Quá gần thì lùi")]
    public float retreatDistance = 2f;

    [Tooltip("Tốc độ lùi")]
    public float retreatSpeed = 2f;

    [Tooltip("Lùi tối đa")]
    public float retreatDuration = 1.5f;


    [Header("Circle Shoot")]

    [Tooltip("Số đạn bắn vòng")]
    public int bulletsInCircle = 12;

    [Tooltip("Thời gian giữa mỗi lần bắn")]
    public float fireRate = 3f;

    public float attackDuration = 0.3f;


    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;


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
        fireTimer -=
            Time.deltaTime;

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
            target.position
        );


        //--------------------------------
        // Quá gần -> lùi
        //--------------------------------

        if (
            distance <
            retreatDistance
        )
        {
            controller.LockMovement(
                true);

            Retreat(dir);

            return;
        }


        //--------------------------------
        // Trong vùng bắn
        //--------------------------------

        if (
        distance <= attackDistance &&
        CanShootPlayer(target)
        )
        {
            controller.LockMovement(true);

            controller.StopMovement();

            controller.PlayAnimation("idle");

            Attack();

            return;
        }

        // Có tường hoặc ngoài tầm -> tiếp tục dùng Seeker
        controller.LockMovement(false);

        isRetreating = false;
        retreatTimer = 0;


        //--------------------------------
        // Ngoài vùng bắn
        //--------------------------------

        controller.LockMovement(
            false);

        isRetreating =
            false;

        retreatTimer =
            0;
    }



    void Retreat(
        Vector2 dir
    )
    {
        isRetreating =
            true;

        retreatTimer +=
            Time.deltaTime;

        Vector2 moveDir =
            -dir.normalized;

        rb.linearVelocity =
            moveDir *
            retreatSpeed;

        controller.PlayAnimation(
            "run"
        );


        if (
            retreatTimer >=
            retreatDuration
        )
        {
            retreatTimer =
                0;

            isRetreating =
                false;

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
            fireTimer =
                fireRate;

            StartCoroutine(
                AttackRoutine()
            );
        }
    }



    IEnumerator AttackRoutine()
    {
        isAttacking =
            true;

        controller.LockMovement(
            true);

        controller.StopMovement();

        controller.PlayAnimation(
            "attack"
        );


        yield return
            new WaitForSeconds(
                attackDuration
            );


        CircleShoot();


        controller.PlayAnimation(
            "idle"
        );

        controller.LockMovement(
            false);

        isAttacking =
            false;
    }



    void CircleShoot()
    {
        if (
            bulletPrefab == null ||
            firePoint == null
        )
            return;


        float angleStep =
            360f /
            bulletsInCircle;


        for (
            int i = 0;
            i < bulletsInCircle;
            i++
        )
        {
            float angle =
                i *
                angleStep;


            Quaternion rot =
                Quaternion.Euler(
                    0,
                    0,
                    angle
                );


            Instantiate(
                bulletPrefab,
                firePoint.position,
                rot
            );
        }
    }

    bool CanShootPlayer(Transform player)
    {
        if (firePoint == null)
            return false;

        Vector2 origin = firePoint.position;

        Vector2 targetPos = player.position;

        Vector2 dir =
            (targetPos - origin).normalized;

        float distance =
            Vector2.Distance(
                origin,
                targetPos);

        RaycastHit2D hit =
            Physics2D.Raycast(
                origin,
                dir,
                distance,
                wallLayer);

        return hit.collider == null;
    }

    private void OnDrawGizmosSelected()
    {
        if (controller != null &&
            controller.HasTarget &&
            firePoint != null)
        {
            Gizmos.color =
                CanShootPlayer(controller.Target)
                ? Color.red
                : Color.gray;

            Gizmos.DrawLine(
                firePoint.position,
                controller.Target.position);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            attackDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(
            transform.position,
            retreatDistance);
    }
}