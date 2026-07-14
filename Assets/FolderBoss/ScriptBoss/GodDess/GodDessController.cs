using UnityEngine;
using System.Collections;

public class GodDessController : BossEndController
{
    [Header("Goddess Skills")]

    public GoddessLaserSkill laserSkill;

    public GoddessMeteorSkill meteorSkill;

    public GoddessCloneSkill cloneSkill;

    public bool isClone;

    int fixedClonePhase = 1;

    protected override void Start()
    {
        base.Start();

        //--------------------------------
        // clone khóa phase
        //--------------------------------

        if (isClone)
        {
            currentPhase =
            fixedClonePhase;
        }
    }

    protected override void UpdatePhase()
    {
        //--------------------------------
        // clone không đổi phase
        //--------------------------------

        if (isClone)
            return;

        base.UpdatePhase();
    }

    //--------------------------------

    protected override IEnumerator Think()
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
        // Nếu player quá gần
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
        // AI riêng Goddess
        //--------------------------------

        else
        {
            int action;

            if (isClone)
            {
                action =
                Random.Range(
                    0,
                    4
                );
            }
            else
            {
                if (currentPhase == 1)
                {
                    action =
                    Random.Range(
                        0,
                        5
                    );
                }

                else if (
                    currentPhase == 2
                )
                {
                    int roll =
                    Random.Range(
                        0,
                        100
                    );

                    if (roll < 20)
                        action = 0;

                    else if (roll < 40)
                        action = 1;

                    else if (roll < 60)
                        action = 2;

                    else if (roll < 80)
                        action = 3;

                    else
                        action = 4;
                }

                else
                {
                    int roll =
                    Random.Range(
                        0,
                        100
                    );

                    if (roll < 10)
                        action = 0;

                    else if (roll < 20)
                        action = 1;

                    else if (roll < 40)
                        action = 2;

                    else if (roll < 60)
                        action = 3;

                    else
                        action = 4;
                }
            }

            //--------------------------------
            // thực thi action
            //--------------------------------

            switch (action)
            {
                case 0:
                    yield return StartCoroutine(
                        WalkToPlayer()
                    );
                    break;

                case 1:
                    yield return StartCoroutine(
                        CircleMove()
                    );
                    break;

                case 2:
                    yield return StartCoroutine(
                        UseLaser()
                    );
                    break;

                case 3:
                    yield return StartCoroutine(
                        UseMeteor()
                    );
                    break;

                case 4:
                    yield return StartCoroutine(
                        UseClone()
                    );
                    break;
            }
        }

        isThinking = false;
    }

    //--------------------------------
    // SKILLS
    //--------------------------------

    IEnumerator UseLaser()
    {
        usingSkill = true;

        PlayAttack();

        yield return
        StartCoroutine(
            laserSkill.Cast()
        );

        usingSkill = false;
    }

    IEnumerator UseMeteor()
    {
        usingSkill = true;

        PlayAttack();

        yield return
        StartCoroutine(
            meteorSkill.Cast()
        );

        usingSkill = false;
    }

    IEnumerator UseClone()
    {
        if (
            isClone ||
            cloneSkill == null
        )
            yield break;

        usingSkill = true;

        try
        {
            PlayAttack();

            yield return StartCoroutine(
                cloneSkill.Cast()
            );
        }

        finally
        {
            usingSkill = false;
        }
    }
}