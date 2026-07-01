using UnityEngine;
using System.Collections;
using Unity.Burst.Intrinsics;

[RequireComponent(typeof(Rigidbody2D))]
public class BossBloodController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;

    [Header("Visual")]
    public Transform bossVisual;

    [Header("Movement")]
    public float moveSpeed = 2f;

    public float detectRange = 10f;
    public float keepDistance = 3f;

    public float randomMoveRadius = 3f;
    public float randomMoveInterval = 2f;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public int bulletCount = 2;
    public float bulletInterval = .5f;

    [Header("Summon")]
    public GameObject summonPrefab;
    public Transform summonPoint;

    public int summonCount = 3;
    public float summonLifeTime = 3f;

    [Header("Skill AI")]
    public float skillInterval = 5f;

    [Header("Death")]
    public float deathDuration = 1.5f;

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
            anim = GetComponent<Animator>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

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

        if (
            !isDead &&
            bossHeath.currentHeath <= 0
        )
        {
            isDead = true;

            StopAllCoroutines();

            rb.linearVelocity =
            Vector2.zero;

            rb.simulated = false;

            foreach (
                Collider2D col
                in GetComponents<Collider2D>()
            )
            {
                col.enabled = false;
            }

            PlayDeath();

            StartCoroutine(
                DeathRoutine()
            );

            return;
        }

        if (
            isDead ||
            target == null
        )
            return;

        //--------------------------------
        // PHASE2
        //--------------------------------

        if (
            !phase2 &&
            bossHeath.currentHeath <=
            bossHeath.maxHeath * .5f
        )
        {
            phase2 = true;

            skillInterval = 2f;

            bulletCount += 2;

            summonCount += 3;

            moveSpeed += 5f;
        }

        //--------------------------------
        // SKILL
        //--------------------------------

        if (!isCastingSkill)
        {
            skillTimer +=
            Time.deltaTime;

            if (skillTimer >=
               skillInterval)
            {
                skillTimer = 0;

                int skill =
                Random.Range(0, 2);

                if (skill == 0)
                {
                    StartCoroutine(
                        AttackSkill()
                    );
                }
                else
                {
                    StartCoroutine(
                        SummonSkill()
                    );
                }
            }
        }

        if (isCastingSkill)
            return;

        //--------------------------------
        // RANGE
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

        if (
            target.position.x >
            transform.position.x
        )
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
        playerPos +
        offset;

        float distance =
        Vector2.Distance(
            randomTarget,
            playerPos
        );

        if (distance <
           keepDistance)
        {
            Vector2 dir =
            (
                randomTarget -
                playerPos
            ).normalized;

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
    // ATTACK
    //--------------------------------

    IEnumerator AttackSkill()
    {
        isCastingSkill = true;

        for (
            int i = 0;
            i < bulletCount;
            i++
        )
        {
            PlayAttack();

            yield return
            new WaitForSeconds(.3f);

            SpawnBullet();

            yield return
            new WaitForSeconds(1f);

            SpawnBullet();

            yield return
            new WaitForSeconds(
                bulletInterval
            );
        }

        PlayIdle();

        isCastingSkill = false;
    }

    void SpawnBullet()
    {
        Vector2 dir =
        (
        target.position -
        firePoint.position
        ).normalized;

        float angle =
        Mathf.Atan2(
            dir.y,
            dir.x
        ) * Mathf.Rad2Deg;

        Instantiate(
            bulletPrefab,
            firePoint.position,
            Quaternion.Euler(
                0,
                0,
                angle
            )
        );
    }

    //--------------------------------
    // SUMMON
    //--------------------------------

    IEnumerator SummonSkill()
    {
        isCastingSkill = true;

        PlaySummon();

        yield return
        new WaitForSeconds(.5f);

        for (
            int i = 0;
            i < summonCount;
            i++
        )
        {
            Vector2 randomPos =
            (Vector2)
            summonPoint.position
            +
            Random.insideUnitCircle
            * 2f;

            GameObject summon =
            Instantiate(
                summonPrefab,
                randomPos,
                Quaternion.identity
            );

            // hủy object vừa sinh
            Destroy(
                summon,
                summonLifeTime
            );
        }

        yield return
        new WaitForSeconds(.5f);

        PlayIdle();

        isCastingSkill = false;
    }

    //--------------------------------
    // ANIMATION
    //--------------------------------

    void PlayIdle()
    {
        anim.Play("idle");
    }

    void PlayAttack()
    {
        anim.Play(
            "attack",
            0,
            0
        );
    }

    void PlaySummon()
    {
        anim.Play(
            "summon",
            0,
            0
        );
    }

    void PlayDeath()
    {
        anim.Play("death");
    }

    //--------------------------------
    // DESTROY
    //--------------------------------

    IEnumerator DeathRoutine()
    {
        yield return
        new WaitForSeconds(
            deathDuration
        );

        Destroy(gameObject);
    }

    //--------------------------------
    // GIZMOS
    //--------------------------------

    void OnDrawGizmosSelected()
    {
        // Detect Range
        Gizmos.color =
        Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );

        // Keep Distance
        Gizmos.color =
        Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            keepDistance
        );

        // Random Move Radius
        Gizmos.color =
        Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            randomMoveRadius
        );

        // Điểm boss muốn đi tới
        Gizmos.color =
        Color.green;

        Gizmos.DrawSphere(
            randomTarget,
            .2f
        );

        // Đường nối tới điểm di chuyển
        Gizmos.color =
        Color.magenta;

        Gizmos.DrawLine(
            transform.position,
            randomTarget
        );

        // Vùng summon
        if (summonPoint != null)
        {
            Gizmos.color =
            Color.blue;

            Gizmos.DrawWireSphere(
                summonPoint.position,
                2f
            );
        }

        // Điểm bắn đạn
        if (
            firePoint != null
        )
        {
            Gizmos.color =
            Color.white;

            Gizmos.DrawSphere(
                firePoint.position,
                .15f
            );
        }
    }
}
