using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossRockController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public BossHeath bossHeath;

    [Header("Visual")]
    public Transform bossVisual;

    [Header("Movement")]
    public float moveSpeed = 2f;

    public float detectRange = 10f;
    public float keepDistance = 4f;

    public float randomMoveRadius = 3f;
    public float randomMoveInterval = 2f;

    [Header("Laser Skill")]
    public GameObject laserObject;

    public float laserDuration = 2f;

    public int laserCastCount = 1; // số lần laser xuất hiện
    public float laserInterval = .5f; // nghỉ giữa mỗi lần

    [Header("Shoot Skill")]
    public GameObject bulletPrefab;

    public Transform shootPoint;

    public int shootCount = 3;

    public float shootInterval = .3f;

    [Header("Skill AI")]
    public float skillInterval = 5f;

    private float skillTimer;
    private float moveTimer;

    private Vector2 randomTarget;

    private bool isCastingSkill;
    private bool isDead;
    private bool phase2;

    private Transform target;
    private Rigidbody2D rb;

    //--------------------------------
    // SETUP
    //--------------------------------

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim =
            GetComponent<Animator>();

        if (bossHeath == null)
            bossHeath =
            GetComponent<BossHeath>();
    }

    void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player != null)
            target =
            player.transform;
    }

    //--------------------------------
    // UPDATE
    //--------------------------------

    void Update()
    {
        //--------------------------------
        // DEATH
        //--------------------------------

        if (!isDead &&
            bossHeath.currentHeath <= 0)
        {
            isDead = true;

            StopAllCoroutines();

            isCastingSkill = false;

            if (laserObject != null)
                laserObject.SetActive(false);

            rb.linearVelocity =
            Vector2.zero;

            rb.simulated = false;

            foreach (
                Collider2D col
                in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            PlayDeath();

            StartCoroutine(
                DeathRoutine()
            );

            return;
        }

        if (isDead ||
            target == null)
            return;

        //--------------------------------
        // PHASE 2
        //--------------------------------

        if (!phase2 &&
        bossHeath.currentHeath <=
        bossHeath.maxHeath * .5f)
        {
            phase2 = true;

            skillInterval = 2f;

            shootCount += 10;

            laserDuration += 1f;

            laserCastCount += 1; // từ 1 -> 2 lần

            moveSpeed += 5f;
        }

        //--------------------------------
        // SKILL TIMER
        //--------------------------------

        if (!isCastingSkill)
        {
            skillTimer += Time.deltaTime;

            if (skillTimer >= skillInterval)
            {
                skillTimer = 0f;

                int skill =
                    Random.Range(0, 2);

                if (skill == 0)
                {
                    StartCoroutine(
                        LaserSkill()
                    );
                }
                else
                {
                    StartCoroutine(
                        ShootSkill()
                    );
                }
            }
        }

        if (isCastingSkill)
            return;

        //--------------------------------
        // PLAYER RANGE
        //--------------------------------

        float distance =
            Vector2.Distance(
                transform.position,
                target.position
            );

        if (distance >
            detectRange)
        {
            PlayIdle();
            return;
        }

        //--------------------------------
        // RANDOM MOVE
        //--------------------------------

        moveTimer -=
            Time.deltaTime;

        if (moveTimer <= 0)
        {
            moveTimer =
                randomMoveInterval;

            PickRandomPosition();
        }

        //--------------------------------
        // LOOK PLAYER
        //--------------------------------

        Vector3 rot =
            bossVisual.localEulerAngles;

        if (target.position.x >
            transform.position.x)
        {
            rot.y = 0;
        }
        else
        {
            rot.y = 180;
        }

        bossVisual.localEulerAngles =
            rot;

        MoveBoss();

        PlayIdle();
    }

    //--------------------------------
    // MOVE
    //--------------------------------

    void PickRandomPosition()
    {
        Vector2 offset =
            Random.insideUnitCircle *
            randomMoveRadius;

        Vector2 playerPos =
            target.position;

        randomTarget =
            playerPos + offset;

        float distance =
            Vector2.Distance(
                randomTarget,
                playerPos
            );

        if (distance <
            keepDistance)
        {
            Vector2 dir =
                (randomTarget -
                playerPos)
                .normalized;

            randomTarget =
                playerPos +
                dir *
                keepDistance;
        }
    }

    void MoveBoss()
    {
        Vector2 dir =
        (
        randomTarget -
        (Vector2)
        transform.position
        ).normalized;

        rb.MovePosition(
            rb.position +
            dir *
            moveSpeed *
            Time.deltaTime
        );
    }

    //--------------------------------
    // LASER
    //--------------------------------

    IEnumerator LaserSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity =
            Vector2.zero;

        for (int i = 0;
             i < laserCastCount;
             i++)
        {
            if (isDead)
                yield break;

            // phát animation mỗi lần
            PlayLaser();

            yield return
            new WaitForSeconds(.5f);

            if (laserObject != null)
                laserObject.SetActive(true);

            // thời gian laser tồn tại
            yield return
            new WaitForSeconds(
                laserDuration
            );

            if (laserObject != null)
                laserObject.SetActive(false);

            // nghỉ giữa các lần bắn
            if (i < laserCastCount - 1)
            {
                yield return
                new WaitForSeconds(
                    laserInterval
                );
            }
        }

        PlayIdle();

        isCastingSkill = false;
    }

    //--------------------------------
    // SHOOT
    //--------------------------------

    IEnumerator ShootSkill()
    {
        isCastingSkill = true;

        for (int i = 0;
            i < shootCount;
            i++)
        {
            if (isDead)
                yield break;

            PlayShoot();

            yield return
            new WaitForSeconds(
                .3f
            );

            SpawnBullet();

            yield return
            new WaitForSeconds(
                shootInterval
            );
        }

        PlayIdle();

        isCastingSkill =
            false;
    }

    void SpawnBullet()
    {
        Vector2 dir =
            (
            target.position -
            shootPoint.position
            ).normalized;

        float angle =
            Mathf.Atan2(
                dir.y,
                dir.x
            ) *
            Mathf.Rad2Deg;

        Instantiate(
            bulletPrefab,
            shootPoint.position,
            Quaternion.Euler(
                0,
                0,
                angle
            )
        );
    }

    //--------------------------------
    // ANIMATION
    //--------------------------------

    void PlayIdle()
    {
        anim.Play("idle");
    }

    void PlayLaser()
    {
        anim.Play(
            "laser_cast",
            0,
            0
        );
    }

    void PlayShoot()
    {
        anim.Play(
            "shoot",
            0,
            0
        );
    }

    void PlayDeath()
    {
        anim.Play(
            "death"
        );
    }

    //--------------------------------
    // DESTROY
    //--------------------------------

    IEnumerator DeathRoutine()
    {
        yield return
        new WaitForSeconds(
            anim.GetCurrentAnimatorStateInfo(0)
            .length
        );

        Destroy(gameObject);
    }

    //--------------------------------
    // GIZMOS
    //--------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );

        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            keepDistance
        );

        Gizmos.color =
            Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            randomMoveRadius
        );
    }
}