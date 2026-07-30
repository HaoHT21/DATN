using UnityEngine;
using System.Collections;

public class BossFireController : BossController
{
    [Header("Fireball")]
    public GameObject fireballPrefab;
    public Transform fireballPoint;

    public int fireballCount = 30;
    public float fireballInterval = 0.01f;
    public float randomAngle = 30f;

    [Header("SpitFire Setup")]
    public GameObject spitFireObject;
    public float telegraphDuration = 0.8f;
    public float spitFireDuration = 1f;
    public int spitFireCount = 1;
    public float spitFireInterval = 0.5f;

    public GameObject aimLaserLine;

    //--------------------------------
    // PHASE
    //--------------------------------

    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            fireballCount += 50;
            spitFireCount += 1;
            randomAngle += 5f;
            moveSpeed += 10;
        }
    }

    protected override void DisableEffects()
    {
        if (spitFireObject != null) spitFireObject.SetActive(false);
        if (aimLaserLine != null) aimLaserLine.SetActive(false);
    }

    protected override IEnumerator UseSkill1()
    {
        yield return FireballSkill();
    }

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

        yield return new WaitForSeconds(Random.Range(thinkMin, thinkMax));

        int action = -1;

        // Phase 1
        if (currentPhase == 1)
        {
            int roll = Random.Range(0, 100);

            if (roll < 30) // 30% Di chuyển
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
        yield return new WaitForSeconds(Random.Range(0.8f, 1.5f));
        isMoving = false;
    }

    //--------------------------------
    // FIREBALL
    //--------------------------------

    IEnumerator FireballSkill()
    {
        usingSkill = true;
        FireBall();

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < fireballCount; i++)
        {
            SpawnFireball();
            yield return new WaitForSeconds(fireballInterval);
        }

        usingSkill = false;
        PlayIdle();
    }

    void SpawnFireball()
    {
        float offset = Random.Range(-randomAngle, randomAngle);
        Instantiate(fireballPrefab, fireballPoint.position, fireballPoint.rotation * Quaternion.Euler(0, 0, offset));
    }

    //--------------------------------
    // SPIT FIRE
    //--------------------------------

    IEnumerator SpitFireSkill()
    {
        usingSkill = true;

        FireLookAtPlayer tracker = spitFireObject.GetComponent<FireLookAtPlayer>();
        if (tracker == null) tracker = GetComponentInChildren<FireLookAtPlayer>();

        for (int i = 0; i < spitFireCount; i++)
        {
            SpitFire();

            // 1. Bật tracker xoay và laser nhắm
            if (tracker != null) tracker.StartTracking();
            if (aimLaserLine != null) aimLaserLine.SetActive(true);

            float timer = 0f;
            bool lostTarget = false;

            // 2. Đếm ngược thời gian ngắm (kiểm tra góc khuất)
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

            // 3. Tắt laser nhắm
            if (aimLaserLine != null) aimLaserLine.SetActive(false);

            // 4. Bật lửa nếu không mất dấu (Lửa chỉ ẩn/hiện, không bị khóa cứng góc)
            if (!lostTarget)
            {
                if (spitFireObject != null) spitFireObject.SetActive(true);

                yield return new WaitForSeconds(spitFireDuration);

                if (spitFireObject != null) spitFireObject.SetActive(false);
            }
            else
            {
                Debug.Log("Player đã chui vào góc khuất! Boss hủy phun lửa.");
            }

            yield return new WaitForSeconds(spitFireInterval);
        }

        usingSkill = false;
        PlayIdle();
    }
}