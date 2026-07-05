using UnityEngine;
using System.Collections;

public class BossSlimeController : BossController
{
    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public int bulletCount = 20;
    public int jumpAttackCount = 2;

    [Header("Summon")]
    public GameObject enemyPrefab;

    public int summonCount = 5;
    public float summonRadius = 3f;
    public float summonLifeTime = 10f;

    int skillIndex;

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(
        int phase
    )
    {
        if (phase == 2)
        {
            bulletCount += 10;
            summonCount += 2;
            jumpAttackCount += 2;

            moveSpeed += 5;
        }

        if (phase == 3)
        {
            bulletCount += 10;
            summonCount += 2;
            jumpAttackCount += 2;

            moveSpeed += 5;
        }
    }

    //--------------------------------
    // SKILL 1
    //--------------------------------

    protected override IEnumerator UseSkill1()
    {
        yield return ShootSkill();
    }

    //--------------------------------
    // SKILL 2
    //--------------------------------

    protected override IEnumerator UseSkill2()
    {
        yield return SummonSkill();
    }

    //--------------------------------
    // boss tự chọn thứ tự
    //--------------------------------

    protected override IEnumerator Think()
    {
        isThinking = true;

        yield return new WaitForSeconds(
            Random.Range(
                thinkMin,
                thinkMax
            )
        );

        int action = 0;

        //--------------------------------
        // Phase 1
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

            if (roll < 50)
            {
                yield return StartCoroutine(
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
        // Phase 2
        //--------------------------------

        else if (currentPhase == 2)
        {
            int roll =
            Random.Range(
                0,
                100
            );

            if (roll < 30)
                yield return StartCoroutine(
                    MoveState()
                );

            //--------------------------------
            // 30% bắn
            //--------------------------------

            else if (roll < 70)
                action = 0;

            //--------------------------------
            // 20% summon
            //--------------------------------

            else
                action = 1;
        }

        //--------------------------------
        // Phase 3
        //--------------------------------

        else
        {
            int roll =
            Random.Range(
                0,
                100
            );

            if (roll < 20)
                yield return StartCoroutine(
                    MoveState()
                );

            else if (roll < 50)
                action = 0;

            else
                action = 1;
        }

        //--------------------------------
        // Execute
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

    //--------------------------------
    // SHOOT
    //--------------------------------

    IEnumerator MoveState()
    {
        movementLocked = false;

        yield return
        new WaitForSeconds(
            Random.Range(
                .8f,
                1.5f
            )
        );
    }

    IEnumerator ShootSkill()
    {
        usingSkill = true;

        for (
            int wave = 0;
            wave < jumpAttackCount;
            wave++
        )
        {
            //--------------------------------
            // phát animation mỗi wave
            //--------------------------------

            PlayAttack();

            yield return
            new WaitForSeconds(.5f);

            float angleStep =
            360f / bulletCount;

            for (
                int i = 0;
                i < bulletCount;
                i++
            )
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

            yield return
            new WaitForSeconds(.4f);
        }

        usingSkill = false;

        PlayIdle();
    }

    //--------------------------------
    // SUMMON
    //--------------------------------

    IEnumerator SummonSkill()
    {
        usingSkill = true;

        PlayAttack();

        yield return
        new WaitForSeconds(.5f);

        for (
            int i = 0;
            i < summonCount;
            i++
        )
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

            Destroy(
            enemy,
            summonLifeTime
            );
        }


        usingSkill = false;

        PlayIdle();
    }
}