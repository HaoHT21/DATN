using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class BossEndController : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public BossHeath bossHeath;
    public Transform bossVisual;

    [Header("Range")]
    public float detectRange = 15f;

    public float keepDistance = 5f;

    public float dangerDistance = 2f;

    [Header("Move")]
    public float moveSpeed = 3f;

    public float dashSpeed = 10f;

    [Header("Thinking")]
    public float thinkMin = .5f;
    public float thinkMax = 1.5f;

    [Header("Phase")]

    public int currentPhase = 1;

    bool changingPhase;

    [Header("Phase 3 Dodge")]

    public LayerMask bulletLayer;

    public LayerMask wallLayer;

    public float detectBulletRadius = 4f;

    public float dodgeDistance = 4f;

    bool dodging;

    [Header("Skills")]
    public BossSkillShoot shootSkill;

    public BossSkillDashShoot dashShootSkill;

    public BossSkillBulletRain bulletRainSkill;

    public BossSkillTeleport teleportSkill;

    protected bool movementLocked;
    protected bool usingSkill;

    protected Rigidbody2D rb;
    protected Transform target;

    protected bool isThinking;
    protected bool isMoving;
    protected bool isDead;

    //--------------------------------

    void Awake()
    {
        rb =
        GetComponent<Rigidbody2D>();

        if (anim == null)
            anim =
            GetComponent<Animator>();
    }

    protected virtual void Start()
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

            LockMovement(true);

            // tắt collider
            foreach
            (
                Collider2D col
                in GetComponents<Collider2D>()
            )
            {
                col.enabled = false;
            }

            PlayDeath();

            StartCoroutine(
                DestroyRoutine()
            );

            return;
        }

        FlipToPlayer();

        //--------------------------------
        // Né đạn ưu tiên cao nhất
        //--------------------------------

        if (
            currentPhase == 3 &&
            !dodging
        )
        {
            DetectDangerBullet();
        }

        //--------------------------------
        // Nếu đang né thì thôi
        //--------------------------------

        if (dodging)
            return;

        //--------------------------------
        // trạng thái khóa
        //--------------------------------

        if (
            movementLocked ||
            usingSkill ||
            isThinking ||
            isMoving
        )
            return;

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

        UpdatePhase();

        StartCoroutine(
            Think()
        );
    }

    //--------------------------------

    protected virtual IEnumerator Think()
    {
        isThinking = true;

        PlayIdle();

        yield return new WaitForSeconds(
            Random.Range(
                thinkMin,
                thinkMax
            )
        );

        float distance =
        Vector2.Distance(
            transform.position,
            target.position
        );

        //--------------------------------
        // Player quá gần
        //--------------------------------

        if (distance < dangerDistance)
        {
            int action =
            Random.Range(0, 2);

            switch (action)
            {
                case 0:

                    yield return
                    StartCoroutine(
                        DashBack()
                    );

                    break;

                case 1:

                    yield return
                    StartCoroutine(
                        WalkBack()
                    );

                    break;
            }
        }

        //--------------------------------
        // Bình thường
        //--------------------------------

        else
        {
            //--------------------------------
            // Phase 1
            //--------------------------------

            if (currentPhase == 1)
            {
                int action =
                Random.Range(0, 7);

                switch (action)
                {
                    case 0:

                        yield return
                        StartCoroutine(
                            WalkToPlayer()
                        );

                        break;

                    case 1:

                        yield return
                        StartCoroutine(
                            CircleMove()
                        );

                        break;

                    case 2:

                        yield return
                        StartCoroutine(
                            DashSide()
                        );

                        break;

                    case 3:

                        yield return
                        StartCoroutine(
                            UseShootSkill()
                        );

                        break;

                    case 4:

                        yield return
                        StartCoroutine(
                            UseDashShootSkill()
                        );

                        break;

                    case 5:

                        yield return
                        StartCoroutine(
                            bulletRainSkill.Cast()
                        );

                        break;

                    case 6:

                        yield return
                        StartCoroutine(
                            UseTeleportSkill()
                        );

                        break;
                }
            }

            //--------------------------------
            // Phase 2+
            //--------------------------------

            else
            {
                int roll =
                Random.Range(
                    0,
                    100
                );

                if (roll < 10)
                {
                    yield return
                    StartCoroutine(
                        WalkToPlayer()
                    );
                }

                else if (roll < 20)
                {
                    yield return
                    StartCoroutine(
                        CircleMove()
                    );
                }

                else if (roll < 40)
                {
                    yield return
                    StartCoroutine(
                        UseShootSkill()
                    );
                }

                else if (roll < 60)
                {
                    yield return
                    StartCoroutine(
                        UseDashShootSkill()
                    );
                }

                else if (roll < 80)
                {
                    yield return
                    StartCoroutine(
                        UseTeleportSkill()
                    );
                }

                else
                {
                    yield return
                    StartCoroutine(
                        bulletRainSkill.Cast()
                    );
                }
            }
        }

        isThinking = false;
    }

    protected virtual void UpdatePhase()
    {
        if (changingPhase)
            return;

        float hpPercent =
        (float)
        bossHeath.currentHeath
        /
        bossHeath.maxHeath;

        //--------------------------------

        if (
            hpPercent <= .35f
            &&
            currentPhase < 3
        )
        {
            StartCoroutine(
                ChangePhase(3)
            );
        }

        else if (
            hpPercent <= .7f
            &&
            currentPhase < 2
        )
        {
            StartCoroutine(
                ChangePhase(2)
            );
        }
    }

    protected IEnumerator ChangePhase(
    int phase
    )
    {
        changingPhase = true;

        currentPhase =
        phase;

        LockMovement(
            true
        );

        PlayAttack();

        yield return
        new WaitForSeconds(
            1f
        );

        //--------------------------------

        if (phase == 2)
        {
            thinkMin = .2f;
            thinkMax = .7f;
        }

        if (phase == 3)
        {
            thinkMin = .1f;
            thinkMax = .4f;
        }

        LockMovement(
            false
        );

        changingPhase = false;
    }

    void DetectDangerBullet()
    {
        Collider2D[] bullets =
        Physics2D.OverlapCircleAll(
            transform.position,
            detectBulletRadius,
            bulletLayer
        );

        foreach (
            Collider2D bullet
            in bullets
        )
        {
            Rigidbody2D bulletRb =
            bullet.GetComponent<Rigidbody2D>();

            if (
                bulletRb == null
            )
                continue;

            //--------------------------------
            // đạn có đang bay về boss không
            //--------------------------------

            Vector2 toBoss =
            (
            transform.position -
            bullet.transform.position
            )
            .normalized;

            float dot =
            Vector2.Dot(
                bulletRb.linearVelocity.normalized,
                toBoss
            );

            if (dot > .8f)
            {
                StartCoroutine(
                    TeleportDodge(bulletRb)
                );

                return;
            }
        }
    }

    protected IEnumerator TeleportDodge(
 Rigidbody2D bulletRb
 )
    {
        dodging = true;
        isMoving = true;

        PlayRun();

        Vector2 bulletDir =
        bulletRb.linearVelocity.normalized;

        Vector2 bestPos =
        rb.position;

        float bestScore =
        -9999f;

        //--------------------------------
        // thử 16 vị trí quanh boss
        //--------------------------------

        for (
            int i = 0;
            i < 16;
            i++
        )
        {
            float angle =
            i * 22.5f;

            Vector2 dir =
            new Vector2(
                Mathf.Cos(
                    angle *
                    Mathf.Deg2Rad
                ),
                Mathf.Sin(
                    angle *
                    Mathf.Deg2Rad
                )
            );

            Vector2 pos =
            rb.position +
            dir *
            dodgeDistance;

            //--------------------------------
            // đụng tường
            //--------------------------------

            Collider2D wall =
            Physics2D.OverlapCircle(
                pos,
                .5f,
                wallLayer
            );

            if (wall != null)
                continue;

            //--------------------------------
            // khoảng cách player
            //--------------------------------

            float playerDist =
            Mathf.Abs(
                Vector2.Distance(
                    pos,
                    target.position
                )
                -
                keepDistance
            );

            //--------------------------------
            // vị trí có nằm
            // trên hướng đạn không
            //--------------------------------

            Vector2 toPos =
            (
                pos -
                bulletRb.position
            ).normalized;

            float bulletDot =
            Vector2.Dot(
                bulletDir,
                toPos
            );

            //--------------------------------
            // nguy hiểm
            //--------------------------------

            float bulletDanger =
            bulletDot > .7f
            ?
            100f
            :
            0f;

            //--------------------------------
            // điểm
            //--------------------------------

            float score =
            -playerDist
            -
            bulletDanger;

            //--------------------------------

            if (
                score >
                bestScore
            )
            {
                bestScore =
                score;

                bestPos =
                pos;
            }
        }

        //--------------------------------
        // teleport
        //--------------------------------

        rb.position =
        bestPos;

        yield return
        new WaitForSeconds(
            .25f
        );

        isMoving = false;
        dodging = false;
    }

    protected IEnumerator UseShootSkill()
    {
        usingSkill = true;

        yield return
        StartCoroutine(
            shootSkill.Cast()
        );

        usingSkill = false;
    }

    //--------------------------------
    // ĐI TỚI
    //--------------------------------

    protected IEnumerator WalkToPlayer()
    {
        isMoving = true;

        PlayRun();

        float timer =
        Random.Range(
            .5f,
            1.5f
        );

        while (timer > 0)
        {
            Vector2 dir =
            (
            target.position -
            transform.position
            ).normalized;

            rb.MovePosition(
                rb.position +
                dir *
                moveSpeed *
                Time.deltaTime
            );

            timer -=
            Time.deltaTime;

            yield return null;
        }

        isMoving = false;
    }

    //--------------------------------
    // ĐI LÙI
    //--------------------------------

    protected IEnumerator WalkBack()
    {
        isMoving = true;

        PlayRun();

        float timer = 1f;

        while (timer > 0)
        {
            Vector2 dir =
            (
            transform.position -
            target.position
            ).normalized;

            rb.MovePosition(
                rb.position +
                dir *
                moveSpeed *
                Time.deltaTime
            );

            timer -=
            Time.deltaTime;

            yield return null;
        }

        isMoving = false;
    }

    //--------------------------------
    // DASH LÙI
    //--------------------------------

    protected IEnumerator DashBack()
    {
        isMoving = true;

        PlayRun();

        Vector2 dir =
        (
        transform.position -
        target.position
        ).normalized;

        float timer = .3f;

        while (timer > 0)
        {
            rb.MovePosition(
                rb.position +
                dir *
                dashSpeed *
                Time.deltaTime
            );

            timer -=
            Time.deltaTime;

            yield return null;
        }

        isMoving = false;
    }

    //--------------------------------
    // DASH NGANG
    //--------------------------------

    protected IEnumerator DashSide()
    {
        isMoving = true;

        PlayRun();

        Vector2 dir =
        Random.value > .5f
        ?
        Vector2.right
        :
        Vector2.left;

        float timer = .3f;

        while (timer > 0)
        {
            rb.MovePosition(
                rb.position +
                dir *
                dashSpeed *
                Time.deltaTime
            );

            timer -=
            Time.deltaTime;

            yield return null;
        }

        isMoving = false;
    }

    //--------------------------------
    // ĐI QUANH
    //--------------------------------

    protected IEnumerator CircleMove()
    {
        isMoving = true;

        PlayRun();

        float timer = 1f;

        Vector2 side =
        Random.value > .5f
        ?
        Vector2.right
        :
        Vector2.left;

        while (timer > 0)
        {
            rb.MovePosition(
                rb.position +
                side *
                moveSpeed *
                Time.deltaTime
            );

            timer -=
            Time.deltaTime;

            yield return null;
        }

        isMoving = false;
    }

    protected IEnumerator UseDashShootSkill()
    {
        usingSkill = true;

        yield return
        StartCoroutine(
            dashShootSkill.Cast()
        );

        usingSkill = false;
    }

    //--------------------------------

    public void LockMovement(
    bool value
)
    {
        movementLocked =
        value;

        if (value)
        {
            rb.linearVelocity =
            Vector2.zero;
        }
    }

    public void FlipToPlayer()
    {
        if (target == null)
            return;

        Vector3 rot =
        bossVisual.localEulerAngles;

        if (
            target.position.x >
            transform.position.x
        )
        {
            rot.y = 0f;
        }
        else
        {
            rot.y = 180f;
        }

        bossVisual.localEulerAngles =
        rot;
    }

    protected IEnumerator UseTeleportSkill()
    {
        usingSkill = true;

        yield return
        StartCoroutine(
            teleportSkill.Cast()
        );

        usingSkill = false;
    }

    protected IEnumerator DestroyRoutine()
    {
        yield return
        new WaitForSeconds(
            2f
        );

        Destroy(
            gameObject
        );
    }

    void PlayDeath()
    {
        anim.Play("death");
    }

    public void PlayIdle()
    {
        anim.Play("idle");
    }

    public void PlayRun()
    {
        anim.Play("run");
    }

    public void PlayAttack()
    {
        anim.Play(
            "attack",
            0,
            0f
        );
    }

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
            dangerDistance
        );

        Gizmos.color =
        Color.green;

        Gizmos.DrawWireSphere(
            transform.position,
            keepDistance
        );

        Gizmos.color =
        Color.magenta;

        Gizmos.DrawWireSphere(
            transform.position,
            detectBulletRadius
        );
    }
}