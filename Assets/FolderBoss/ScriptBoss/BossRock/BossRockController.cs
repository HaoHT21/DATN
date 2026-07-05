using UnityEngine;
using System.Collections;

public class BossRockController : BossController
{
    [Header("Laser")]
    public GameObject laserObject;

    public float laserDuration = 2f;
    public int laserCastCount = 1;
    public float laserInterval = .5f;

    [Header("Shoot")]
    public GameObject bulletPrefab;

    public Transform shootPoint;
    public int shootCount = 3;
    public float shootInterval = .3f;

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(
        int phase
    )
    {
        //--------------------------------
        // phase 2
        //--------------------------------

        if (phase == 2)
        {
            shootCount += 5;

            laserCastCount += 1;

            moveSpeed += 5;
        }

        //--------------------------------
        // phase 3
        //--------------------------------

        if (phase == 3)
        {
            shootCount += 5;

            laserCastCount += 1;

            moveSpeed += 5;
        }
    }

    protected override void DisableEffects()
    {
        if (laserObject != null)
        {
            laserObject.SetActive(
                false
            );
        }
    }

    //--------------------------------
    // Skill 1
    //--------------------------------

    protected override IEnumerator UseSkill1()
    {
        yield return LaserSkill();
    }

    //--------------------------------
    // Skill 2
    //--------------------------------

    protected override IEnumerator UseSkill2()
    {
        yield return ShootSkill();
    }

    //--------------------------------
    // THINK
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
            {
                yield return StartCoroutine(
                    MoveState()
                );
            }

            else if (roll < 65)
            {
                action = 0;
            }

            else
            {
                action = 1;
            }
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
            {
                yield return StartCoroutine(
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
    // LASER
    //--------------------------------

    IEnumerator LaserSkill()
    {
        usingSkill = true;

        for (
            int i = 0;
            i < laserCastCount;
            i++
        )
        {
            Laser();

            yield return
            new WaitForSeconds(
                .5f
            );

            laserObject.SetActive(
                true
            );

            yield return
            new WaitForSeconds(
                laserDuration
            );

            laserObject.SetActive(
                false
            );

            yield return
            new WaitForSeconds(
                laserInterval
            );
        }

        usingSkill = false;

        PlayIdle();
    }

    //--------------------------------
    // SHOOT
    //--------------------------------

    IEnumerator ShootSkill()
    {
        usingSkill = true;

        for (
            int i = 0;
            i < shootCount;
            i++
        )
        {
            Shoot();

            yield return
            new WaitForSeconds(
                .3f
            );

            Vector2 dir =
            (
                target.position -
                shootPoint.position
            ).normalized;

            float angle =
            Mathf.Atan2(
                dir.y,
                dir.x
            ) *
            Mathf.Rad2Deg;

            Instantiate(
                bulletPrefab,
                shootPoint.position,
                Quaternion.Euler(
                    0,
                    0,
                    angle
                )
            );

            yield return
            new WaitForSeconds(
                shootInterval
            );
        }

        usingSkill = false;

        PlayIdle();
    }
}