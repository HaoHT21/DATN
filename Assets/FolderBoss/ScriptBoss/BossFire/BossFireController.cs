using UnityEngine;
using System.Collections;

public class BossFireController : BossController
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;
    public Transform fireballPoint;
    public int fireballCount = 30;
    public float fireballInterval = 0.01f;
    public float randomAngle = 30f;

    [Header("SpitFire Settings")]
    public GameObject spitFireObject;
    public float telegraphDuration = 0.8f;
    public float spitFireDuration = 1.2f;  // Thời gian vừa phun vừa quét hết 90 độ
    public float sweepAngle = 90f;          // Tổng góc quét (90 độ)
    public int spitFireCount = 1;
    public float spitFireInterval = 0.5f;
    public GameObject aimLaserLine;

    private BossFireAudio _bossAudio; // Cache component audio

    protected override void Awake()
    {
        base.Awake();
        _bossAudio = GetComponent<BossFireAudio>();
    }

    // ------------------------------------------------
    // 1. ĐĂNG KÝ SKILL CHO HỆ THỐNG WEIGHT
    // ------------------------------------------------
    protected override void RegisterBossSkills()
    {
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 40, 30));
        bossSkills.Add(new SkillWeight(FireballSkill, 30, 30));
        bossSkills.Add(new SkillWeight(SpitFireSkill, 30, 40));
    }

    // ------------------------------------------------
    // 2. XỬ LÝ CHUYỂN PHASE & TẮT EFFECT
    // ------------------------------------------------
    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            fireballCount += 50;
            spitFireCount += 1;
            randomAngle += 5f;
            moveSpeed += 10f;
        }
    }

    protected override void DisableEffects()
    {
        base.DisableEffects();

        if (spitFireObject != null) spitFireObject.SetActive(false);
        if (aimLaserLine != null) aimLaserLine.SetActive(false);
    }

    // ------------------------------------------------
    // 3. SKILL 1: FIREBALL (MƯA CẦU LỬA)
    // ------------------------------------------------
    private IEnumerator FireballSkill()
    {
        // Cho phép Boss lướt né đạn trong khi đang xả đạn mưa cầu lửa
        canDodgeDuringSkill = true;

        FireBall();
        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < fireballCount; i++)
        {
            SpawnFireball();
            yield return new WaitForSeconds(fireballInterval);
        }
        // Tắt quyền né đạn sau khi xong skill
        canDodgeDuringSkill = false;
    }

    private void SpawnFireball()
    {
        if (fireballPrefab == null || fireballPoint == null) return;

        float offset = Random.Range(-randomAngle, randomAngle);
        Instantiate(fireballPrefab, fireballPoint.position, fireballPoint.rotation * Quaternion.Euler(0, 0, offset));

        // GOI ÂM THANH BẮN CẦU LỬA DIRECTLY
        if (_bossAudio != null)
        {
            _bossAudio.PlayFireballSound(fireballPoint.position);
        }
    }

    // ------------------------------------------------
    // 4. SKILL 2: SPIT FIRE QUÉT GÓC 90 ĐỘ
    // ------------------------------------------------
    private IEnumerator SpitFireSkill()
    {
        canDodgeDuringSkill = false;
        if (bossHeath != null) bossHeath.isInvincible = true; // Bất tử trong khi phun lửa
        FireLookAtPlayer tracker = spitFireObject != null ? spitFireObject.GetComponent<FireLookAtPlayer>() : null;
        if (tracker == null) tracker = GetComponentInChildren<FireLookAtPlayer>();

        try
        {
            for (int i = 0; i < spitFireCount; i++)
            {
                SpitFire();

                // STEP 1: BẬT NGẮM (Telegraph)
                if (tracker != null) tracker.StartTracking();
                if (aimLaserLine != null) aimLaserLine.SetActive(true);

                float timer = 0f;
                bool lostTarget = false;

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

                // STEP 2: CHỐT/KHÓA VỊ TRÍ
                // Tắt laser ngắm và gọi StopTracking() để chốt góc lockedBaseAngle
                if (aimLaserLine != null) aimLaserLine.SetActive(false);
                if (tracker != null) tracker.StopTracking();

                // STEP 3: PHUN LỬA & QUÉT TRÊN GÓC ĐÃ KHÓA
                if (!lostTarget)
                {
                    if (spitFireObject != null) spitFireObject.SetActive(true);

                    float halfAngle = sweepAngle / 2f;
                    float startAngle = -halfAngle;
                    float endAngle = halfAngle;

                    // Chọn ngẫu nhiên hướng quét: Trái -> Phải hoặc Phải -> Trái
                    if (Random.value > 0.5f)
                    {
                        startAngle = halfAngle;
                        endAngle = -halfAngle;
                    }

                    float sweepTimer = 0f;
                    while (sweepTimer < spitFireDuration)
                    {
                        sweepTimer += Time.deltaTime;
                        float progress = sweepTimer / spitFireDuration;

                        // Tính offset góc dựa trên thời gian quét
                        float currentOffset = Mathf.Lerp(startAngle, endAngle, progress);

                        // Xoay tia lửa theo góc offset + góc gốc đã khóa
                        if (tracker != null)
                        {
                            tracker.SetSweepAngle(currentOffset);
                        }

                        yield return null;
                    }

                    if (spitFireObject != null) spitFireObject.SetActive(false);

                    // DỪNG TIẾNG PHUN LỬA LẶP
                    if (_bossAudio != null) _bossAudio.StopSpitFireLoop();
                }
                else
                {
                    Debug.Log("Player đã vào góc khuất! Hủy kỹ năng.");
                }

                yield return new WaitForSeconds(spitFireInterval);
            }
        }
        finally
        {
            if (bossHeath != null) bossHeath.isInvincible = false; // Hủy bất tử sau khi phun lửa xong
        }
    }
}