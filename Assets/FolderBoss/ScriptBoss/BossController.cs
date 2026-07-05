using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class BossController : MonoBehaviour
{
    [Header("Random Move")]
    public float randomMoveRadius = 3f;
    public float randomMoveInterval = 2f;

    protected Vector2 randomTarget;
    protected float moveTimer;

    [Header("References")]
    public Animator anim;
    public BossHeath bossHeath;
    public Transform bossVisual;
    public SpriteRenderer spriteRenderer;
    public Collider2D[] hitColliders;
    public SpriteRenderer[] sprites;

    [Header("Movement")]
    public float moveSpeed = 3f;
    public float detectRange = 15f;
    public float keepDistance = 5f;

    [Header("Thinking")]
    public float thinkMin = .5f;
    public float thinkMax = 1.5f;

    protected Rigidbody2D rb;
    protected Transform target;

    protected bool isDead;
    protected bool isMoving;
    protected bool isThinking;
    protected bool usingSkill;
    protected bool movementLocked;
    protected MoveType currentMove;

    protected int currentPhase = 1;

    //--------------------------------

    //--------------------------------
    // Skill cho boss con override
    //--------------------------------

    protected abstract IEnumerator UseSkill1();

    protected abstract IEnumerator UseSkill2();

    protected virtual void Awake()
    {
        rb =
        GetComponent<Rigidbody2D>();

        if (anim == null)
            anim =
            GetComponent<Animator>();
        if (spriteRenderer == null)
            spriteRenderer =
            GetComponent<SpriteRenderer>();
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

    protected virtual void Update()
    {
        MoveLogic();

        UpdatePhase();
        CheckDeath();

        if (
            isDead ||
            target == null
        )
            return;

        FlipToPlayer();

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

        yield return new WaitForSeconds(
            Random.Range(
                thinkMin,
                thinkMax
            )
        );

        //--------------------------------
        // phase 1
        //--------------------------------

        if (currentPhase == 1)
        {
            int roll =
            Random.Range(
                0,
                100
            );

            //--------------------------------
            // 70% di chuyển
            //--------------------------------

            if (roll < 70)
            {
                yield return
                StartCoroutine(
                    MoveState()
                );
            }

            //--------------------------------
            // 15%
            //--------------------------------

            else if (roll < 85)
            {
                yield return
                StartCoroutine(
                    UseSkill1()
                );
            }

            //--------------------------------
            // 15%
            //--------------------------------

            else
            {
                yield return
                StartCoroutine(
                    UseSkill2()
                );
            }
        }

        //--------------------------------
        // phase 2
        //--------------------------------

        else if (currentPhase == 2)
        {
            int roll =
            Random.Range(
                0,
                100
            );

            if (roll < 50)
            {
                yield return
                StartCoroutine(
                    MoveState()
                );
            }

            else if (roll < 75)
            {
                yield return
                StartCoroutine(
                    UseSkill1()
                );
            }

            else
            {
                yield return
                StartCoroutine(
                    UseSkill2()
                );
            }
        }

        //--------------------------------
        // phase 3
        //--------------------------------

        else
        {
            int roll =
            Random.Range(
                0,
                100
            );

            if (roll < 30)
            {
                yield return
                StartCoroutine(
                    MoveState()
                );
            }

            else if (roll < 65)
            {
                yield return
                StartCoroutine(
                    UseSkill1()
                );
            }

            else
            {
                yield return
                StartCoroutine(
                    UseSkill2()
                );
            }
        }

        isThinking = false;
    }

    IEnumerator MoveState()
    {
        isMoving = true;

        yield return new WaitForSeconds(
            Random.Range(
                .8f,
                1.5f
            )
        );

        isMoving = false;
    }

    //--------------------------------
    // Movement AI
    //--------------------------------
    public enum MoveType
    {
        KeepDistance,
        Approach
    }

    protected virtual void MoveLogic()
    {
        if (
            target == null ||
            movementLocked
        )
            return;

        moveTimer -= Time.deltaTime;

        if (moveTimer <= 0)
        {
            moveTimer =
            randomMoveInterval;

            int roll =
            Random.Range(
                0,
                100
            );

            currentMove =
            roll < 70
            ?
            MoveType.KeepDistance
            :
            MoveType.Approach;
        }

        MoveBoss();
    }

    protected virtual void PickRandomPosition()
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

        //--------------------------------
        // giữ khoảng cách với player
        //--------------------------------

        if (distance < keepDistance)
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

    protected virtual void MoveBoss()
    {
        Vector2 dir =
        Vector2.zero;

        Vector2 toPlayer =
        (
            target.position -
            transform.position
        ).normalized;

        float distance =
        Vector2.Distance(
            transform.position,
            target.position
        );

        //--------------------------------
        // Giữ khoảng cách
        //--------------------------------

        if (currentMove ==
            MoveType.KeepDistance)
        {
            float offset =
            distance -
            keepDistance;

            dir =
            toPlayer *
            offset;
        }

        //--------------------------------
        // Áp sát
        //--------------------------------

        else if (
            currentMove ==
            MoveType.Approach)
        {
            dir =
            toPlayer;
        }

        rb.MovePosition(
            rb.position +
            dir.normalized *
            moveSpeed *
            Time.deltaTime
        );
    }

    //--------------------------------
    // Override ở boss con
    //--------------------------------

    protected abstract void OnPhaseChange(
        int phase
    );

    //--------------------------------

    protected virtual void UpdatePhase()
    {
        float hpPercent =
        (float)
        bossHeath.currentHeath /
        bossHeath.maxHeath;

        if (
            hpPercent <= .35f &&
            currentPhase < 3
        )
        {
            currentPhase = 3;

            OnPhaseChange(3);
        }

        else if (
            hpPercent <= .7f &&
            currentPhase < 2
        )
        {
            currentPhase = 2;

            OnPhaseChange(2);
        }
    }

    //--------------------------------

    protected virtual void CheckDeath()
    {
        if (
            isDead ||
            bossHeath.currentHeath > 0
        )
            return;

        isDead = true;

        StopAllCoroutines();

        DisableEffects();

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
            DestroyRoutine()
        );
    }

    IEnumerator DestroyRoutine()
    {
        yield return
        new WaitForSeconds(
            2f
        );

        Destroy(gameObject);
    }

    //--------------------------------

    protected void MoveTo(
        Vector2 position
    )
    {
        Vector2 dir =
        (
            position -
            rb.position
        ).normalized;

        rb.MovePosition(
            rb.position +
            dir *
            moveSpeed *
            Time.deltaTime
        );
    }

    //--------------------------------

    protected void FlipToPlayer()
    {
        Vector3 rot =
        bossVisual.localEulerAngles;

        rot.y =
        target.position.x >
        transform.position.x
        ?
        0
        :
        180;

        bossVisual.localEulerAngles =
        rot;
    }

    protected virtual void DisableEffects()
    {
    }

    //--------------------------------

    protected void PlayIdle()
    {
        anim.Play("idle");
    }

    protected void PlayAttack()
    {
        anim.Play(
            "attack",
            0,
            0
        );
    }

    protected void Shoot()
    {
        anim.Play(
            "shoot",
            0,
            0
        );
    }

    protected void Laser()
    {
        anim.Play(
            "laser_cast",
            0,
            0
        );
    }

    protected void RedBull()
    {
        anim.Play("redbull");
    }

    protected void Ice()
    {
        anim.Play(
            "ice",
            0,
            0
        );
    }

    protected void FireBall()
    {
        anim.Play("fireball");
    }

    protected void SpitFire()
    {
        anim.Play("spitfire");
    }

    protected void Summon()
    {
        anim.Play("summon");
    }
    protected void Fly()
    {
        anim.Play(
            "fly",
            0,
            0
            );
    }

    protected void PlayDeath()
    {
        anim.Play("death");
    }

    //--------------------------------
    // GIZMOS
    //--------------------------------

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

        // Điểm boss đang muốn tới
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(
            randomTarget,
            .25f
        );

        // Đường nối boss → điểm đích
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(
            transform.position,
            randomTarget
        );
    }
}