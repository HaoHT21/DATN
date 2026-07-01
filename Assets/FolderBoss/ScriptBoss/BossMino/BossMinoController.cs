using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossMinoController : MonoBehaviour
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

    public int bulletCount = 5;
    public int shootCount = 2;
    public float shootInterval = .5f;

    [Header("RedBull Skill")]
    public GameObject redBullEffect;

    public float chargeSpeed = 10f;
    public float chargeDuration = 2f;

    public int redBullCount = 1;
    public float redBullInterval = .5f;

    [Header("Retreat")]
    public float retreatDistance = 6f;
    public float retreatSpeed = 4f;

    [Header("Skill AI")]
    public float skillInterval = 5f;

    [Header("Visual")]
    public Transform bossVisual;

    [Header("Death")]
    public float deathDuration = 1.5f;

    private float skillTimer;
    private float moveTimer;

    private Vector2 randomTarget;
    private Vector2 chargeDirection;

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

        if (redBullEffect != null)
            redBullEffect.SetActive(false);
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

            if (redBullEffect != null)
                redBullEffect.SetActive(false);

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
            shootCount += 2;

            redBullCount += 1;

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
                        ShootSkill()
                    );
                }
                else
                {
                    StartCoroutine(
                        RedBullSkill()
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
    // SHOOT
    //--------------------------------

    IEnumerator ShootSkill()
    {
        isCastingSkill = true;

        for (int i = 0; i < shootCount; i++)
        {
            // chạy animation 1 lần
            PlayAttack();

            // đợi tới thời điểm đòn 1
            yield return
            new WaitForSeconds(.3f);

            FireBullets();

            // đợi tới thời điểm đòn 2
            yield return
            new WaitForSeconds(.5f);

            FireBullets();

            // nghỉ trước combo tiếp theo
            yield return
            new WaitForSeconds(
                shootInterval
            );
        }

        PlayIdle();

        isCastingSkill = false;
    }

    private void FireBullets()
    {
        if (bulletPrefab == null ||
            firePoint == null)
            return;

        float spread = 45f;

        // nếu chỉ có 1 viên
        if (bulletCount <= 1)
        {
            Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            return;
        }

        // nhiều viên tỏa hình quạt
        for (int i = 0; i < bulletCount; i++)
        {
            float angle =
                -spread / 2f +
                (spread / (bulletCount - 1)) * i;

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

    //--------------------------------
    // REDBULL
    //--------------------------------

    IEnumerator RedBullSkill()
    {
        isCastingSkill = true;

        for (int i = 0;
             i < redBullCount;
             i++)
        {
            // cập nhật hướng nhìn mới
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

            // lấy hướng mới tới player
            chargeDirection =
            (
            target.position -
            transform.position
            ).normalized;

            PlayRedBull();

            yield return
            new WaitForSeconds(.3f);

            if (redBullEffect != null)
                redBullEffect.SetActive(
                    true
                );

            float timer = 0;

            while (
                timer <
                chargeDuration)
            {
                rb.MovePosition(
                    rb.position +
                    chargeDirection *
                    chargeSpeed *
                    Time.fixedDeltaTime
                );

                timer +=
                    Time.fixedDeltaTime;

                yield return
                new WaitForFixedUpdate();
            }

            rb.linearVelocity =
            Vector2.zero;

            if (redBullEffect != null)
                redBullEffect.SetActive(false);

            // lùi ra sau khi húc
            yield return
            StartCoroutine(
                RetreatAfterCharge()
            );

            // nghỉ giữa các lần
            if (i < redBullCount - 1)
            {
                yield return
                new WaitForSeconds(
                    redBullInterval
                );
            }
        }

        PlayIdle();

        isCastingSkill = false;
    }

    IEnumerator RetreatAfterCharge()
    {
        float distance =
            Vector2.Distance(
                transform.position,
                target.position
            );

        // nếu đủ xa thì thôi
        if (distance >= retreatDistance)
            yield break;

        while (distance < retreatDistance)
        {
            if (target == null)
                yield break;

            // hướng ngược player
            Vector2 dir =
            (
            transform.position -
            target.position
            ).normalized;

            // flip theo hướng lùi
            Vector3 rot =
                bossVisual.localEulerAngles;

            if (dir.x > 0)
                rot.y = 0f;
            else
                rot.y = 180f;

            bossVisual.localEulerAngles =
                rot;

            rb.MovePosition(
                rb.position +
                dir *
                retreatSpeed *
                Time.fixedDeltaTime
            );

            distance =
                Vector2.Distance(
                    transform.position,
                    target.position
                );

            yield return
                new WaitForFixedUpdate();
        }

        rb.linearVelocity =
            Vector2.zero;
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

    void PlayRedBull()
    {
        anim.Play(
            "redbull",
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
        yield return
            new WaitForSeconds(
                deathDuration
            );

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Detect Range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            detectRange
        );

        // Keep Distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            keepDistance
        );

        // Random Move Radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            randomMoveRadius
        );

        // Điểm boss đang muốn di chuyển tới
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            randomTarget,
            .2f
        );

        // Đường nối tới mục tiêu di chuyển
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(
            transform.position,
            randomTarget
        );

        // Khi đang charge RedBull
        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;

            Gizmos.DrawRay(
                transform.position,
                chargeDirection * 3f
            );
        }

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            retreatDistance
        );
    }
}