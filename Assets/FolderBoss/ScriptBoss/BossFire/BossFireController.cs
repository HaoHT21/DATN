using UnityEngine;
using System.Collections;

public class BossFireController : BossController
{
    [Header("Fireball")]
    public GameObject fireballPrefab;
    public Transform fireballPoint;

    public int fireballCount = 30;
    public float fireballInterval = .08f;
    public float randomAngle = 30f;

    [Header("SpitFire")]
    public GameObject spitFireObject;

    public float spitFireDuration = 1f;

    public int spitFireCount = 1;
    public float spitFireInterval = .5f;

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(
        int phase
    )
    {
        if (phase == 2)
        {
            fireballCount += 30;

            spitFireCount += 1;

            randomAngle -= 10f;

            moveSpeed += 5;
        }

        if (phase == 3)
        {
            fireballCount += 50;

            spitFireCount += 1;

            moveSpeed += 5;
        }
    }

    //--------------------------------
    // EFFECT OFF WHEN DEAD
    //--------------------------------

    protected override void DisableEffects()
    {
        if (spitFireObject != null)
        {
            spitFireObject.SetActive(
                false
            );
        }
    }

    //--------------------------------
    // SKILL 1
    //--------------------------------

    protected override IEnumerator UseSkill1()
    {
        yield return FireballSkill();
    }

    //--------------------------------
    // SKILL 2
    //--------------------------------

    protected override IEnumerator UseSkill2()
    {
        yield return SpitFireSkill();
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
    // FIREBALL
    //--------------------------------

    IEnumerator FireballSkill()
    {
        usingSkill = true;

        FireBall();

        yield return
        new WaitForSeconds(
            .5f
        );

        for (
            int i = 0;
            i < fireballCount;
            i++
        )
        {
            SpawnFireball();

            yield return
            new WaitForSeconds(
                fireballInterval
            );
        }

        usingSkill = false;

        PlayIdle();
    }

    void SpawnFireball()
    {
        float offset =
        Random.Range(
            -randomAngle,
            randomAngle
        );

        Instantiate(
            fireballPrefab,
            fireballPoint.position,
            fireballPoint.rotation *
            Quaternion.Euler(
                0,
                0,
                offset
            )
        );
    }

    //--------------------------------
    // SPIT FIRE
    //--------------------------------

    IEnumerator SpitFireSkill()
    {
        usingSkill = true;

        for (
            int i = 0;
            i < spitFireCount;
            i++
        )
        {
            SpitFire();

            yield return
            new WaitForSeconds(
                .5f
            );

            if (spitFireObject != null)
            {
                spitFireObject.SetActive(
                    true
                );
            }

            yield return
            new WaitForSeconds(
                spitFireDuration
            );

            if (spitFireObject != null)
            {
                spitFireObject.SetActive(
                    false
                );
            }

            yield return
            new WaitForSeconds(
                spitFireInterval
            );
        }

        usingSkill = false;

        PlayIdle();
    }
}