using UnityEngine;
using System.Collections;

public class BossRockController : BossController
{
    [Header("Laser Setup")]
    public GameObject laserObject;
    public float telegraphDuration = 0.8f; // Thời gian bật laser định vị ngắm Player
    public float laserDuration = 2f;      // Thời gian thực sự xả laser
    public int laserCastCount = 1;
    public float laserInterval = .5f;
    public GameObject aimLaserLine;       // Đường laser ngắm (telegraph warning)

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public int shootCount = 3;
    public float shootInterval = .3f;

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            shootCount += 5;
            laserCastCount += 1;
            moveSpeed += 5;
        }
    }

    protected override void DisableEffects()
    {
        if (laserObject != null) laserObject.SetActive(false);
        if (aimLaserLine != null) aimLaserLine.SetActive(false);
    }

    //--------------------------------
    // SKILLS
    //--------------------------------

    protected override IEnumerator UseSkill1()
    {
        yield return LaserSkill();
    }

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

        yield return new WaitForSeconds(Random.Range(thinkMin, thinkMax));

        int action = -1;

        // Phase 1
        if (currentPhase == 1)
        {
            int roll = Random.Range(0, 100);

            if (roll < 30)
            {
                yield return StartCoroutine(MoveState());
            }
            else if (roll < 70)
            {
                action = 0;
            }
            else
            {
                action = 1;
            }
        }
        // Phase 2
        else
        {
            int roll = Random.Range(0, 100);

            if (roll < 20)
            {
                yield return StartCoroutine(MoveState());
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
                yield return StartCoroutine(UseSkill1());
                break;
            case 1:
                yield return StartCoroutine(UseSkill2());
                break;
        }

        isThinking = false;
    }

    IEnumerator MoveState()
    {
        isMoving = true;
        yield return new WaitForSeconds(Random.Range(.8f, 1.5f));
        isMoving = false;
    }

    //--------------------------------
    // LASER (Đã đồng bộ logic với SpitFire của BossFire)
    //--------------------------------

    IEnumerator LaserSkill()
    {
        usingSkill = true;

        // Lấy tracker xoay từ laserObject hoặc các con của nó
        FireLookAtPlayer tracker = laserObject.GetComponent<FireLookAtPlayer>();
        if (tracker == null) tracker = GetComponentInChildren<FireLookAtPlayer>();

        for (int i = 0; i < laserCastCount; i++)
        {
            Laser();

            // 1. Bật tracker xoay và tia ngắm cảnh báo
            if (tracker != null) tracker.StartTracking();
            if (aimLaserLine != null) aimLaserLine.SetActive(true);

            float timer = 0f;
            bool lostTarget = false;

            // 2. Đếm ngược thời gian ngắm (kiểm tra Player có chui vào góc khuất không)
            while (timer < telegraphDuration)
            {
                timer += Time.deltaTime;

                if (tracker != null && !tracker.CanSeePlayer())
                {
                    lostTarget = true;
                    break;
                }

                yield return null;
            }

            // 3. Tắt tia ngắm cảnh báo
            if (aimLaserLine != null) aimLaserLine.SetActive(false);

            // 4. Bật Laser gây xát thương nếu không bị mất dấu
            if (!lostTarget)
            {
                if (laserObject != null) laserObject.SetActive(true);

                yield return new WaitForSeconds(laserDuration);

                if (laserObject != null) laserObject.SetActive(false);
            }
            else
            {
                Debug.Log("Player đã chui vào góc khuất! Boss Rock hủy bắn Laser.");
            }

            yield return new WaitForSeconds(laserInterval);
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

        for (int i = 0; i < shootCount; i++)
        {
            Shoot();

            yield return new WaitForSeconds(.3f);

            if (target != null && shootPoint != null)
            {
                Vector2 dir = (target.position - shootPoint.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                Instantiate(bulletPrefab, shootPoint.position, Quaternion.Euler(0, 0, angle));
            }

            yield return new WaitForSeconds(shootInterval);
        }

        usingSkill = false;
        PlayIdle();
    }
}