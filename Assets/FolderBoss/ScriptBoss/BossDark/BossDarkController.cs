using UnityEngine;
using System.Collections;

public class BossDarkController : BossController
{
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

    [Header("Invisible")]
    public LayerMask wallLayer;

    BoxCollider2D bodyCollider;

    bool isInvisible;

    //--------------------------------
    // SETUP
    //--------------------------------

    protected override void Awake()
    {
        base.Awake();

        bodyCollider =
        GetComponent<BoxCollider2D>();

        if (
            sprites == null ||
            sprites.Length == 0
        )
        {
            sprites =
            GetComponentsInChildren<
            SpriteRenderer>();
        }

        if (
            hitColliders == null ||
            hitColliders.Length == 0
        )
        {
            hitColliders =
            GetComponentsInChildren<
            Collider2D>();
        }
    }

    protected void SetInvisible(
bool value
)
    {
        foreach (
            SpriteRenderer sp
            in sprites
        )
        {
            sp.enabled =
            !value;
        }

        foreach (
            Collider2D col
            in hitColliders
        )
        {
            col.enabled =
            !value;
        }

        if (anim != null)
        {
            anim.enabled =
            !value;
        }
    }

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(
        int phase
    )
    {
        if (phase == 2)
        {
            castCount += 2;

            spawnBulletCount += 10;

            moveSpeed += 5;
        }
    }

    //--------------------------------
    // SKILL1
    //--------------------------------

    protected override IEnumerator UseSkill1()
    {
        yield return CastSkill();
    }

    //--------------------------------
    // SKILL2
    //--------------------------------

    protected override IEnumerator UseSkill2()
    {
        yield return InvisibleSkill();
    }

    //--------------------------------
    // THINK
    //--------------------------------

    protected override IEnumerator Think()
    {
        isThinking = true;

        yield return
        new WaitForSeconds(
            Random.Range(
                thinkMin,
                thinkMax
            )
        );

        int action = 0;

        if (currentPhase == 1)
        {
            int roll =
            Random.Range(0, 100);

            if (roll < 30)
            {
                yield return
                StartCoroutine(
                    MoveState()
                );
            }

            else if (roll < 60)
            {
                action = 0;
            }

            else
            {
                action = 1;
            }
        }

        else
        {
            int roll =
            Random.Range(0, 100);

            if (roll < 20)
            {
                yield return
                StartCoroutine(
                    MoveState()
                );
            }

            else if (roll < 60)
            {
                action = 0;
            }

            else
            {
                action = 1;
            }
        }

        switch (action)
        {
            case 0:

                yield return
                StartCoroutine(
                    UseSkill1()
                );

                break;

            case 1:

                yield return
                StartCoroutine(
                    UseSkill2()
                );

                break;
        }

        isThinking = false;
    }

    IEnumerator MoveState()
    {
        yield return
        new WaitForSeconds(
            Random.Range(
                .8f,
                1.5f
            )
        );
    }

    //--------------------------------
    // CAST
    //--------------------------------

    IEnumerator CastSkill()
    {
        usingSkill = true;

        for (
            int i = 0;
            i < castCount;
            i++
        )
        {
            Cast();

            yield return
            new WaitForSeconds(
                .3f
            );

            Instantiate(
                bulletCastPrefab,
                castPoint.position,
                castPoint.rotation
            );

            yield return
            new WaitForSeconds(
                castInterval
            );
        }

        usingSkill = false;

        PlayIdle();
    }

    //--------------------------------
    // INVISIBLE
    //--------------------------------

    IEnumerator InvisibleSkill()
    {
        usingSkill = true;

        isInvisible = true;

        SetInvisible(true);

        PickInvisibleTarget();

        float timer = 0;

        while (
            timer <
            invisibleDuration
        )
        {
            MoveInvisible();

            timer +=
            Time.deltaTime;

            yield return null;
        }

        SetInvisible(false);

        SpawnCircleBullet();

        PlayIdle();

        isInvisible = false;

        usingSkill = false;
    }

    //--------------------------------
    // INVISIBLE MOVE
    //--------------------------------

    void MoveInvisible()
    {
        if (
            Vector2.Distance(
                transform.position,
                randomTarget
            ) < .5f
        )
        {
            PickInvisibleTarget();
        }

        Vector2 dir =
        (
            randomTarget -
            rb.position
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
            (Vector2)transform.position +
            Random.insideUnitCircle *
            randomMoveRadius;

            //--------------------------------
            // điểm nằm trong tường
            //--------------------------------

            if (
                Physics2D.OverlapCircle(
                    pos,
                    .5f,
                    wallLayer
                )
            )
            {
                continue;
            }

            //--------------------------------
            // đường đi xuyên tường
            //--------------------------------

            RaycastHit2D hit =
            Physics2D.Linecast(
                transform.position,
                pos,
                wallLayer
            );

            if (hit.collider != null)
            {
                continue;
            }

            //--------------------------------

            randomTarget = pos;
            return;
        }

        randomTarget =
        transform.position;
    }

    //--------------------------------
    // BULLET CIRCLE
    //--------------------------------

    void SpawnCircleBullet()
    {
        for (
            int i = 0;
            i < spawnBulletCount;
            i++
        )
        {
            float angle =
            (360f /
            spawnBulletCount)
            * i;

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
}