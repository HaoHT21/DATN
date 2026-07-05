using UnityEngine;
using System.Collections;

public class BossBloodController : BossController
{
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

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(
        int phase
    )
    {
        if (phase == 2)
        {
            bulletCount += 1;

            summonCount += 2;

            moveSpeed += 5;
        }

        if (phase == 3)
        {
            bulletCount += 3;

            summonCount += 2;

            moveSpeed += 5;
        }
    }

    //--------------------------------
    // SKILL 1
    //--------------------------------

    protected override IEnumerator UseSkill1()
    {
        yield return AttackSkill();
    }

    //--------------------------------
    // SKILL 2
    //--------------------------------

    protected override IEnumerator UseSkill2()
    {
        yield return SummonSkill();
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

        //--------------------------------
        // Phase1
        //--------------------------------

        if (currentPhase == 1)
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
                action = 0;
            }

            else
            {
                action = 1;
            }
        }

        //--------------------------------
        // Phase2
        //--------------------------------

        else if (currentPhase == 2)
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

            else if (roll < 60)
            {
                action = 0;
            }

            else
            {
                action = 1;
            }
        }

        //--------------------------------
        // Phase3
        //--------------------------------

        else
        {
            int roll =
            Random.Range(
                0,
                100
            );

            if (roll < 20)
            {
                yield return
                StartCoroutine(
                    MoveState()
                );
            }

            else if (roll < 50)
            {
                action = 0;
            }

            else
            {
                action = 1;
            }
        }

        //--------------------------------

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
    // ATTACK
    //--------------------------------

    IEnumerator AttackSkill()
    {
        usingSkill = true;

        for (
            int i = 0;
            i < bulletCount;
            i++
        )
        {
            PlayAttack();

            yield return
            new WaitForSeconds(
                .3f
            );

            SpawnBullet();

            yield return
            new WaitForSeconds(
                1f
            );

            SpawnBullet();

            yield return
            new WaitForSeconds(
                bulletInterval
            );
        }

        usingSkill = false;

        PlayIdle();
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
        ) *
        Mathf.Rad2Deg;

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
        usingSkill = true;

        Summon();

        yield return
        new WaitForSeconds(
            .5f
        );

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

            Destroy(
                summon,
                summonLifeTime
            );
        }

        yield return
        new WaitForSeconds(
            .5f
        );

        usingSkill = false;

        PlayIdle();
    }
}