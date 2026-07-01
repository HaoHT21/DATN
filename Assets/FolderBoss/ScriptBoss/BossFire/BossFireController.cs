using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossFireController : MonoBehaviour
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

    [Header("Fireball Skill")]
    public GameObject fireballPrefab;
    public Transform fireballPoint;

    public int fireballCount = 30;
    public float fireballInterval = .08f;
    public float randomAngle = 30f;

    [Header("Spit Fire Skill")]
    public GameObject spitFireObject;

    public float spitFireDuration = 1f;

    public int spitFireCount = 1; // số lần phun
    public float spitFireInterval = .5f; // nghỉ giữa các lần

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

            // tắt skill đang bật
            if (spitFireObject != null)
                spitFireObject.SetActive(false);

            // dừng di chuyển
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;

            // tắt collider
            foreach (Collider2D col in GetComponents<Collider2D>())
            {
                col.enabled = false;
            }

            // chạy animation chết
            PlayDeath();

            // bắt đầu hủy boss
            StartCoroutine(DeathRoutine());

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

            fireballCount += 30;

            //spitFireDuration += 1f;

            spitFireCount += 1; // 1 -> 2 lần

            moveSpeed += 5f;

            randomAngle -= 10f;
        }

        //--------------------------------
        // SKILL
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
                        FireballSkill()
                    );
                }
                else
                {
                    StartCoroutine(
                        SpitFireSkill()
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

        //--------------------------------
        // LOOK PLAYER
        //--------------------------------

        Vector3 rot =
            bossVisual.localEulerAngles;

        if (target.position.x >
           transform.position.x)
        {
            rot.y = 0f;
        }
        else
        {
            rot.y = 180f;
        }

        bossVisual.localEulerAngles =
            rot;

        MoveBoss();

        PlayIdle();
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

    IEnumerator FireballSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity =
            Vector2.zero;

        PlayFireball();

        yield return
            new WaitForSeconds(.5f);

        for (int i = 0;
            i < fireballCount;
            i++)
        {
            if (isDead)
                yield break;

            SpawnFireball();

            yield return
            new WaitForSeconds(
                fireballInterval
            );
        }

        PlayIdle();

        isCastingSkill = false;
    }

    void SpawnFireball()
    {
        float offset =
            Random.Range(
                -randomAngle,
                randomAngle
            );

        Instantiate(
            fireballPrefab,
            fireballPoint.position,
            fireballPoint.rotation *
            Quaternion.Euler(
                0,
                0,
                offset
            )
        );
    }

    IEnumerator SpitFireSkill()
    {
        isCastingSkill = true;

        rb.linearVelocity =
            Vector2.zero;

        for (int i = 0;
             i < spitFireCount;
             i++)
        {
            if (isDead)
            {
                isCastingSkill = false;
                yield break;
            }

            // chạy animation mỗi lần
            PlaySpitFire();

            yield return
            new WaitForSeconds(.5f);

            // bật lửa
            if (spitFireObject != null)
                spitFireObject.SetActive(
                    true
                );

            // thời gian tồn tại
            yield return
            new WaitForSeconds(
                spitFireDuration
            );

            // tắt lửa
            if (spitFireObject != null)
                spitFireObject.SetActive(
                    false
                );

            // nghỉ giữa các lần
            if (i < spitFireCount - 1)
            {
                yield return
                new WaitForSeconds(
                    spitFireInterval
                );
            }
        }

        PlayIdle();

        isCastingSkill = false;
    }
    void PlayIdle()
    {
        anim.Play("idle");
    }

    void PlayFireball()
    {
        anim.Play(
            "fireball",
            0,
            0
        );
    }

    void PlaySpitFire()
    {
        anim.Play(
            "spitfire",
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
        // lấy độ dài animation death
        AnimatorStateInfo state =
            anim.GetCurrentAnimatorStateInfo(0);

        yield return new WaitForSeconds(
            state.length
        );

        Destroy(gameObject);
    }

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

        // Điểm boss muốn đi tới
        Gizmos.color =
            Color.green;

        Gizmos.DrawSphere(
            randomTarget,
            .2f
        );

        // Đường nối boss -> mục tiêu
        Gizmos.color =
            Color.magenta;

        Gizmos.DrawLine(
            transform.position,
            randomTarget
        );
    }
}