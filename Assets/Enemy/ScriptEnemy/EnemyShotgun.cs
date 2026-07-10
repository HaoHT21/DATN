using UnityEngine;
using System.Collections;

public class EnemyShotgun : MonoBehaviour
{
    public enum ShotMode
    {
        Shotgun,
        RandomRain
    }

    [Header("Mode")]
    public ShotMode shotMode;

    [Header("Distance")]

    [Tooltip("Khoảng cách đứng bắn")]
    public float attackDistance = 5f;

    [Tooltip("Quá gần thì lùi")]
    public float retreatDistance = 2f;

    [Tooltip("Tốc độ lùi")]
    public float retreatSpeed = 2f;

    [Tooltip("Lùi tối đa")]
    public float retreatDuration = 1.5f;


    [Header("Attack")]

    [Tooltip("Cooldown giữa mỗi đợt")]
    public float fireRate = 3f;

    [Tooltip("Thời gian attack animation")]
    public float attackDuration = 0.3f;


    [Header("Burst")]

    [Tooltip("Số lần bắn liên tiếp")]
    public int burstCount = 3;

    [Tooltip("Khoảng nghỉ giữa từng phát")]
    public float burstInterval = 0.2f;


    [Header("Shotgun")]

    [Tooltip("Số đạn mỗi phát")]
    public int bulletsPerShot = 5;

    [Tooltip("Độ rộng chữ V")]
    public float spreadAngle = 45f;

    [Header("Random Rain")]

    [Tooltip("Tổng số viên bắn liên tục")]
    public int randomBulletCount = 20;

    [Tooltip("Khoảng nghỉ giữa từng viên")]
    public float bulletInterval = 0.05f;


    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;


    private EnemyController controller;
    private Rigidbody2D rb;

    private bool isAttacking;
    private bool isRetreating;

    private float retreatTimer;
    private float fireTimer;


    void Awake()
    {
        controller =
            GetComponent<EnemyController>();

        rb =
            GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        if (controller.IsHurting)
            return;

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
                true
            );

            rb.linearVelocity =
                -dir.normalized *
                retreatSpeed;

            controller.PlayAnimation(
                "run"
            );

            return;
        }

        //--------------------------------
        // Giữ khoảng cách + bắn
        //--------------------------------

        if (
            distance <=
            attackDistance
        )
        {
            controller.LockMovement(
                true
            );

            controller.StopMovement();

            controller.PlayAnimation(
                "idle"
            );

            Attack();

            return;
        }

        //--------------------------------
        // Ngoài vùng -> cho chase
        //--------------------------------

        controller.LockMovement(
            false
        );
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

        if (
            fireTimer <= 0
        )
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
            true
        );

        controller.StopMovement();

        for (
            int i = 0;
            i < burstCount;
            i++
        )
        {
            controller.PlayAnimation(
                "attack"
            );

            yield return
                new WaitForSeconds(
                    attackDuration
                );

            //--------------------------------
            // Chọn chế độ
            //--------------------------------

            switch (shotMode)
            {
                case ShotMode.Shotgun:

                    ShootShotgun();

                    break;


                case ShotMode.RandomRain:

                    yield return StartCoroutine(
                        ShootRandomV()
                    );

                    break;
            }

            if (
                i <
                burstCount - 1
            )
            {
                yield return
                    new WaitForSeconds(
                        burstInterval
                    );
            }
        }

        controller.PlayAnimation(
            "idle"
        );

        controller.LockMovement(
            false
        );

        isAttacking =
            false;
    }



    IEnumerator ShootRandomV()
    {
        if (
            bulletPrefab == null ||
            firePoint == null
        )
            yield break;


        for (
            int i = 0;
            i < randomBulletCount;
            i++
        )
        {
            //--------------------------------
            // Random góc trong hình chữ V
            //--------------------------------

            float angle =
                Random.Range(
                    -spreadAngle * 0.5f,
                    spreadAngle * 0.5f
                );


            Quaternion rot =
                firePoint.rotation *
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


            yield return
                new WaitForSeconds(
                    bulletInterval
                );
        }
    }

    void ShootShotgun()
    {
        if (
            bulletPrefab == null ||
            firePoint == null
        )
            return;


        float startAngle =
            -spreadAngle * 0.5f;

        float step =
            bulletsPerShot > 1
            ?
            spreadAngle /
            (
                bulletsPerShot - 1
            )
            :
            0;


        for (
            int i = 0;
            i < bulletsPerShot;
            i++
        )
        {
            float angle =
                startAngle +
                step * i;

            Quaternion rot =
                firePoint.rotation *
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackDistance
        );


        Gizmos.color =
            Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            retreatDistance
        );
    }
}