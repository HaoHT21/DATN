using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossSlimeController : MonoBehaviour
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

    [Header("Shoot Skill")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int bulletCount = 8;

    public int jumpAttackCount = 2;

    [Header("Summon Skill")]
    public GameObject enemyPrefab;
    public int summonCount = 5;
    public float summonRadius = 3f;

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = GetComponent<Animator>();

        if (sprite == null)
            sprite = GetComponent<SpriteRenderer>();

        if (bossHeath == null)
            bossHeath = GetComponent<BossHeath>();
    }

    void Start()
    {
        GameObject player =
            GameObject.FindGameObjectWithTag(
                "Player"
            );

        if (player != null)
            target = player.transform;
    }

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

            // dừng di chuyển
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            // tắt collider để không nhận đạn nữa
            Collider2D col =
                GetComponent<Collider2D>();

            if (col != null)
                col.enabled = false;

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
        bossHeath.maxHeath * 0.5f)
        {
            phase2 = true;

            skillInterval = 3f;

            bulletCount += 6;

            summonCount += 5;

            jumpAttackCount += 2;

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
                        ShootSkill()
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
        // DETECT PLAYER
        //--------------------------------

        float distance =
            Vector2.Distance(
                transform.position,
                target.position
            );

        if (distance > detectRange)
        {
            PlayIdle();
            return;
        }

        //--------------------------------
        // RANDOM MOVE
        //--------------------------------

        moveTimer -= Time.deltaTime;

        if (moveTimer <= 0)
        {
            moveTimer =
                randomMoveInterval;

            PickRandomPosition();
        }

        MoveBoss();

        PlayIdle();

        sprite.flipX =
            target.position.x <
            transform.position.x;
    }

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
                target.position
            );

        if (distance < keepDistance)
        {
            Vector2 dir =
                (randomTarget - playerPos)
                .normalized;

            randomTarget =
                playerPos +
                dir *
                keepDistance;
        }
    }

    void MoveBoss()
    {
        Vector2 direction =
            (
            randomTarget -
            (Vector2)transform.position
            ).normalized;

        rb.MovePosition(
            rb.position +
            direction *
            moveSpeed *
            Time.deltaTime
        );
    }

    IEnumerator ShootSkill()
    {
        isCastingSkill = true;

        for (int wave = 0;
             wave < jumpAttackCount;
             wave++)
        {
            if (isDead)
            {
                isCastingSkill = false;
                yield break;
            }

            PlayAttack();

            yield return new WaitForSeconds(.5f);

            if (isDead)
                yield break;

            float angleStep =
                360f / bulletCount;

            for (int i = 0;
                 i < bulletCount;
                 i++)
            {
                Instantiate(
                    bulletPrefab,
                    firePoint.position,
                    Quaternion.Euler(
                        0,
                        0,
                        i * angleStep
                    )
                );
            }

            yield return new WaitForSeconds(.4f);
        }

        isCastingSkill = false;
    }

    IEnumerator SummonSkill()
    {
        isCastingSkill = true;

        PlayAttack();

        yield return new WaitForSeconds(.5f);

        if (isDead)
        {
            isCastingSkill = false;
            yield break;
        }

        for (int i = 0;
             i < summonCount;
             i++)
        {
            Vector2 pos =
                (Vector2)transform.position +
                Random.insideUnitCircle *
                summonRadius;

            GameObject enemy =
                Instantiate(
                    enemyPrefab,
                    pos,
                    Quaternion.identity
                );

            Destroy(enemy, 10f);
        }

        yield return new WaitForSeconds(.5f);

        isCastingSkill = false;
    }

    void PlayIdle()
    {
        anim.Play("idle");
    }

    void PlayAttack()
    {
        anim.Play("attack", 0, 0f);
    }

    void PlayDeath()
    {
        anim.Play("death");
    }

    IEnumerator DeathRoutine()
    {
        // lấy độ dài animation death
        AnimatorStateInfo state =
            anim.GetCurrentAnimatorStateInfo(0);

        yield return new WaitForSeconds(
            state.length
        );

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        // Detect range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );

        // Keep distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            keepDistance
        );

        // Random move radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            randomMoveRadius
        );
    }
}