using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossDarkController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;
    public BoxCollider2D bodyCollider;

    [Header("Movement")]
    public float moveSpeed = 5f;

    public float detectRange = 10f;
    public float keepDistance = 4f;

    public float randomMoveRadius = 6f;
    public float randomMoveInterval = 2f;

    [Header("Cast Skill")]
    public GameObject bulletCastPrefab;
    public Transform castPoint;

    public int castCount = 3;
    public float castInterval = .5f;

    [Header("Invisible Skill")]
    public float invisibleDuration = 4f;
    public float invisibleMoveSpeed = 20f;

    [Header("Spawn Attack")]
    public GameObject spawnBulletPrefab;
    public Transform spawnPoint;
    public int spawnBulletCount = 12;

    [Header("Skill AI")]
    public float skillInterval = 5f;

    [Header("Invisible")]
    public LayerMask wallLayer;

    [Header("Visual")]
    public Transform bossVisual;

    private float skillTimer;
    private float moveTimer;

    private Vector2 randomTarget;

    private bool isCastingSkill;
    private bool isInvisible;
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

        if (bodyCollider == null)
            bodyCollider =
            GetComponent<BoxCollider2D>();
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

            rb.linearVelocity =
            Vector2.zero;

            rb.simulated = false;

            PlayDeath();

            StartCoroutine(
                DeathRoutine()
            );

            return;
        }

        if (isDead || target == null)
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

            castCount += 5;

            moveSpeed += 5f;

            invisibleMoveSpeed += 10f;

            spawnBulletCount += 20;
        }

        //--------------------------------
        // INVISIBLE MOVE
        //--------------------------------

        if (isInvisible)
        {
            MoveInvisible();
            return;
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
                        CastSkill()
                    );
                }
                else
                {
                    StartCoroutine(
                        InvisibleSkill()
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
            rot.y = 0;
        else
            rot.y = 180;

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
    // CAST SKILL
    //--------------------------------

    IEnumerator CastSkill()
    {
        isCastingSkill = true;

        PlayCast();

        yield return
        new WaitForSeconds(
            .5f
        );

        for (int i = 0;
            i < castCount;
            i++)
        {
            SpawnCast();

            yield return
            new WaitForSeconds(
                castInterval
            );
        }

        PlayIdle();

        isCastingSkill = false;
    }

    void SpawnCast()
    {
        Instantiate(
            bulletCastPrefab,
            castPoint.position,
            castPoint.rotation
        );
    }

    void SpawnCircleBullet()
    {
        if (spawnBulletPrefab == null ||
            spawnPoint == null)
            return;

        for (int i = 0;
             i < spawnBulletCount;
             i++)
        {
            float angle =
                (360f / spawnBulletCount) * i;

            Quaternion rot =
                Quaternion.Euler(
                    0,
                    0,
                    angle
                );

            Instantiate(
                spawnBulletPrefab,
                spawnPoint.position,
                rot
            );
        }
    }

    //--------------------------------
    // INVISIBLE
    //--------------------------------

    IEnumerator InvisibleSkill()
    {
        isCastingSkill = true;
        isInvisible = true;

        sprite.enabled = false;

        bodyCollider.enabled = false;

        anim.enabled = false;

        PickInvisibleTarget();

        yield return
        new WaitForSeconds(
            invisibleDuration
        );

        sprite.enabled = true;

        bodyCollider.enabled = true;

        anim.enabled = true;

        // hiện ra nổ đạn
        SpawnCircleBullet();

        PlayIdle();

        isInvisible = false;
        isCastingSkill = false;
    }

    void MoveInvisible()
    {
        if (
            Vector2.Distance(
                transform.position,
                randomTarget
            ) < 0.5f)
        {
            PickInvisibleTarget();
        }

        Vector2 dir =
        (
            randomTarget -
            (Vector2)
            transform.position
        ).normalized;

        rb.MovePosition(
            rb.position +
            dir *
            invisibleMoveSpeed *
            Time.deltaTime
        );
    }

    void PickInvisibleTarget()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector2 pos =
            (Vector2)
            transform.position +
            Random.insideUnitCircle *
            randomMoveRadius;

            Collider2D wall =
            Physics2D.OverlapCircle(
                pos,
                .5f,
                wallLayer
            );

            if (wall == null)
            {
                randomTarget =
                pos;
                return;
            }
        }
    }

    //--------------------------------
    // ANIMATION
    //--------------------------------

    void PlayIdle()
    {
        anim.Play("idle");
    }

    void PlayCast()
    {
        anim.Play(
            "cast",
            0,
            0
        );
    }

    void PlayDeath()
    {
        anim.Play("death");
    }

    IEnumerator DeathRoutine()
    {
        AnimatorStateInfo state =
            anim.GetCurrentAnimatorStateInfo(0);

        yield return
        new WaitForSeconds(
            state.length
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

        Gizmos.color =
        Color.green;

        Gizmos.DrawSphere(
            randomTarget,
            .2f
        );

        Gizmos.color =
        Color.magenta;

        Gizmos.DrawLine(
            transform.position,
            randomTarget
        );

        if (spawnPoint != null)
        {
            Gizmos.color =
                Color.blue;

            Gizmos.DrawSphere(
                spawnPoint.position,
                .2f
            );
        }
    }
}

