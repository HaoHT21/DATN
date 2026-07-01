using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossAtlantisController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public SpriteRenderer sprite;
    public BossHeath bossHeath;

[Header("Movement")]
    public float moveSpeed = 2f;

    public float detectRange = 10f;
    public float keepDistance = 4f;

    public float randomMoveRadius = 3f;
    public float randomMoveInterval = 2f;

    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public int bulletCount = 3;
    public float bulletInterval = .3f;

    [Header("Fly Skill")]
    public GameObject flyBulletPrefab;

    public int flyBulletCount = 10;
    public float flyDuration = 3f;
    public float flyRadius = 6f;
    public float flySpawnInterval = .3f;

    [Header("Skill AI")]
    public float skillInterval = 5f;

    [Header("Visual")]
    public Transform bossVisual;

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

        if (!isDead &&
           bossHeath.currentHeath <= 0)
        {
            isDead = true;

            StopAllCoroutines();

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

            bulletCount += 5;

            moveSpeed += 2f;

            flyBulletCount += 25;
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
                skillTimer = 0f;

                int randomSkill =
                Random.Range(0, 2);

                if (randomSkill == 0)
                {
                    StartCoroutine(
                        AttackSkill()
                    );
                }
                else
                {
                    StartCoroutine(
                        FlySkill()
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

        for (int i = 0;
             i < bulletCount;
             i++)
        {
            // reset animation mỗi lần bắn
            PlayAttack();

            // chờ tới frame bắn
            yield return
            new WaitForSeconds(.3f);

            SpawnBullet();

            // nghỉ giữa các viên
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
    // FLY SKILL
    //--------------------------------

    IEnumerator FlySkill()
    {
        isCastingSkill = true;

        PlayFly();

        float timer = 0f;
        int spawned = 0;

        PickFlyPosition();

        while (
            timer < flyDuration
            &&
            spawned < flyBulletCount
        )
        {
            SpawnFlyBullet();

            spawned++;

            timer +=
            flySpawnInterval;

            yield return
            new WaitForSeconds(
                flySpawnInterval
            );
        }

        PlayIdle();

        isCastingSkill = false;
    }

    void SpawnFlyBullet()
    {
        if (flyBulletPrefab == null)
            return;

        Vector2 pos =
        (Vector2)transform.position
        +
        Random.insideUnitCircle
        * flyRadius;

        Instantiate(
            flyBulletPrefab,
            pos,
            Quaternion.identity
        );
    }

    void PickFlyPosition()
    {
        Vector2 offset =
        Random.insideUnitCircle *
        randomMoveRadius;

        randomTarget =
        (Vector2)transform.position
        +
        offset;
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

    void PlayFly()
    {
        anim.Play(
            "fly",
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

        Gizmos.color =
        Color.blue;

        Gizmos.DrawWireSphere(
            transform.position,
            flyRadius
        );
    }
}
