using UnityEngine;
using System.Collections;

public class BossMinoController : BossController
{
    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public int bulletCount = 5;
    public int shootCount = 2;
    public float shootInterval = .5f;

    [Header("RedBull")]
    public GameObject redBullEffect;

    public float chargeSpeed = 10f;
    public float chargeDuration = 2f;

    public int redBullCount = 1;
    public float redBullInterval = .5f;

    public float retreatDistance = 6f;
    public float retreatSpeed =  4f;

    Vector2 chargeDirection;

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(
        int phase
    )
    {
        if (phase == 2)
        {
            bulletCount += 3;

            shootCount += 2;

            chargeDuration -= 0.2f;

            redBullCount += 1;

            moveSpeed += 5;
        }

    }

    protected override void DisableEffects()
    {
        if (redBullEffect != null)
        {
            redBullEffect.SetActive(
                false
            );
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
        yield return RedBullSkill();
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
            PlayAttack();

            yield return
            new WaitForSeconds(
                .3f
            );

            FireBullets();

            yield return
            new WaitForSeconds(
                .5f
            );

            FireBullets();

            yield return
            new WaitForSeconds(
                shootInterval
            );
        }

        usingSkill = false;

        PlayIdle();
    }

    void FireBullets()
    {
        float spread = 60f;

        if (bulletCount <= 1)
        {
            Instantiate(
                bulletPrefab,
                firePoint.position,
                firePoint.rotation
            );

            return;
        }

        for (
            int i = 0;
            i < bulletCount;
            i++
        )
        {
            float angle =
            -spread / 2f +
            (
            spread /
            (bulletCount - 1)
            ) * i;

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
        usingSkill = true;
        movementLocked = true;

        for (
            int i = 0;
            i < redBullCount;
            i++
        )
        {
            chargeDirection =
            (
                target.position -
                transform.position
            ).normalized;

            RedBull();

            yield return
            new WaitForSeconds(.3f);

            if (redBullEffect != null)
            {
                redBullEffect.SetActive(
                    true
                );
            }

            float timer = 0;

            while (
                timer <
                chargeDuration
            )
            {
                rb.MovePosition(
                    rb.position +
                    chargeDirection *
                    chargeSpeed *
                    Time.deltaTime
                );

                timer +=
                Time.deltaTime;

                yield return null;
            }

            if (redBullEffect != null)
            {
                redBullEffect.SetActive(
                    false
                );
            }

            yield return
            RetreatAfterCharge();

            yield return
            new WaitForSeconds(
                redBullInterval
            );
        }

        movementLocked = false;
        usingSkill = false;

        PlayIdle();
    }

    IEnumerator RetreatAfterCharge()
    {
        while (
            Vector2.Distance(
                transform.position,
                target.position
            )
            <
            retreatDistance
        )
        {
            Vector2 dir =
            (
            transform.position -
            target.position
            ).normalized;

            rb.MovePosition(
                rb.position +
                dir *
                retreatSpeed *
                Time.deltaTime
            );

            yield return null;
        }
    }
}