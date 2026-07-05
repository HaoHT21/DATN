using UnityEngine;
using System.Collections;

public class BossAtlantisController : BossController
{
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

            flyBulletCount += 20;

            moveSpeed += 5;
        }

        if (phase == 3)
        {
            bulletCount += 1;

            flyBulletCount += 25;

            moveSpeed += 5;
        }
    }

    //--------------------------------
    // SKILL1
    //--------------------------------

    protected override IEnumerator UseSkill1()
    {
        yield return AttackSkill();
    }

    //--------------------------------
    // SKILL2
    //--------------------------------

    protected override IEnumerator UseSkill2()
    {
        yield return FlySkill();
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

        else if (currentPhase == 2)
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

            else if (roll < 50)
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
    // FLY SKILL
    //--------------------------------

    IEnumerator FlySkill()
    {
        usingSkill = true;

        Fly();

        float timer = 0;
        int spawned = 0;

        while (
            timer <
            flyDuration
            &&
            spawned <
            flyBulletCount
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

        usingSkill = false;

        PlayIdle();
    }

    void SpawnFlyBullet()
    {
        Vector2 pos =
        (Vector2)
        transform.position
        +
        Random.insideUnitCircle
        *
        flyRadius;

        Instantiate(
            flyBulletPrefab,
            pos,
            Quaternion.identity
        );
    }
}