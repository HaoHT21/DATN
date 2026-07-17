using UnityEngine;
using System.Collections;

public class BossIceController : BossController
{
    [Header("Ice Burst")]
    public GameObject icePrefab;
    public Transform icePoint;

    public int iceBurstCount = 1;
    public int iceCount = 20;

    [Header("Attack Ice")]
    public GameObject attackIcePrefab;
    public Transform attackIcePoint;

    public int attackIceCount = 3;
    public float attackIceInterval = .3f;

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(
        int phase
    )
    {
        if (phase == 2)
        {
            iceBurstCount += 2;

            iceCount += 10;

            attackIceCount += 2;

            moveSpeed += 5;
        }

    }

    //--------------------------------
    // SKILL 1
    //--------------------------------

    protected override IEnumerator UseSkill1()
    {
        yield return IceSkill();
    }

    //--------------------------------
    // SKILL 2
    //--------------------------------

    protected override IEnumerator UseSkill2()
    {
        yield return AttackIceSkill();
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
        // PHASE 1
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
        // PHASE 2
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
        // EXECUTE
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
    // ICE BURST
    //--------------------------------

    IEnumerator IceSkill()
    {
        usingSkill = true;

        for (
            int wave = 0;
            wave < iceBurstCount;
            wave++
        )
        {
            Ice();

            yield return
            new WaitForSeconds(
                .5f
            );

            float angleStep =
            360f / iceCount;

            for (
                int i = 0;
                i < iceCount;
                i++
            )
            {
                float angle =
                i * angleStep;

                Instantiate(
                    icePrefab,
                    icePoint.position,
                    Quaternion.Euler(
                        0,
                        0,
                        angle
                    )
                );
            }

            yield return
            new WaitForSeconds(
                .4f
            );
        }

        usingSkill = false;

        PlayIdle();
    }

    //--------------------------------
    // ATTACK ICE
    //--------------------------------

    IEnumerator AttackIceSkill()
    {
        usingSkill = true;

        for (
            int i = 0;
            i < attackIceCount;
            i++
        )
        {
            PlayAttack();

            yield return
            new WaitForSeconds(
                .3f
            );

            SpawnAttackIce();

            yield return
            new WaitForSeconds(
                attackIceInterval
            );
        }

        usingSkill = false;

        PlayIdle();
    }

    void SpawnAttackIce()
    {
        Vector2 dir =
        (
            target.position -
            attackIcePoint.position
        ).normalized;

        float angle =
        Mathf.Atan2(
            dir.y,
            dir.x
        ) *
        Mathf.Rad2Deg;

        Instantiate(
            attackIcePrefab,
            attackIcePoint.position,
            Quaternion.Euler(
                0,
                0,
                angle
            )
        );
    }
}