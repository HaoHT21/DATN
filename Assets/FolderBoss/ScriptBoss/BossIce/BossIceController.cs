using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossIceController : MonoBehaviour
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

    [Header("Ice Burst")]
    public GameObject icePrefab;
    public Transform icePoint;

    public int iceCount = 20;

    [Header("Ice Skill")]
    public int iceBurstCount = 1;

    [Header("Attack Ice")]
    public GameObject attackIcePrefab;
    public Transform attackIcePoint;

    public int attackIceCount = 3;
    public float attackIceInterval = .3f;

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
            anim = GetComponent<Animator>();

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

            iceBurstCount += 1;

            iceCount += 20;

            attackIceCount += 10;

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

                int randomSkill =
                    Random.Range(0, 2);

                if (randomSkill == 0)
                {
                    StartCoroutine(
                        IceSkill()
                    );
                }
                else
                {
                    StartCoroutine(
                        AttackIceSkill()
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
    // SKILLS
    //--------------------------------

    IEnumerator IceSkill()
    {
        isCastingSkill = true;

        for (int wave = 0;
             wave < iceBurstCount;
             wave++)
        {
            if (isDead)
            {
                isCastingSkill = false;
                yield break;
            }

            // chạy animation mỗi lần
            PlayIce();

            yield return
            new WaitForSeconds(.5f);

            if (isDead)
            {
                isCastingSkill = false;
                yield break;
            }

            float angleStep =
                360f / iceCount;

            for (int i = 0;
                 i < iceCount;
                 i++)
            {
                float angle =
                    i * angleStep;

                Instantiate(
                    icePrefab,
                    icePoint.position,
                    Quaternion.Euler(
                        0,
                        0,
                        angle
                    )
                );
            }

            // nghỉ giữa các lần bắn
            yield return
            new WaitForSeconds(.4f);
        }

        PlayIdle();

        isCastingSkill = false;
    }

    IEnumerator AttackIceSkill()
    {
        isCastingSkill = true;

        for (int i = 0;
            i < attackIceCount;
            i++)
        {
            if (isDead)
                yield break;

            PlayAttack();

            yield return
            new WaitForSeconds(.3f);

            SpawnAttackIce();

            yield return
            new WaitForSeconds(
                attackIceInterval
            );
        }

        PlayIdle();

        isCastingSkill = false;
    }

    void SpawnAttackIce()
    {
        Vector2 dir =
            (
            target.position -
            attackIcePoint.position
            ).normalized;

        float angle =
            Mathf.Atan2(
                dir.y,
                dir.x
            ) *
            Mathf.Rad2Deg;

        Instantiate(
            attackIcePrefab,
            attackIcePoint.position,
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

    void PlayAttack()
    {
        anim.Play(
            "attack",
            0,
            0
        );
    }

    void PlayIce()
    {
        anim.Play(
            "ice",
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
