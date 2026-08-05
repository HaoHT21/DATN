using UnityEngine;
using System.Collections;

public class BossRockController : BossController
{
    [Header("Laser Setup")]
    public GameObject laserObject;
    public float telegraphDuration = 0.8f;
    public float laserDuration = 2f;
    public int laserCastCount = 1;
    public float laserInterval = 0.5f;
    public GameObject aimLaserLine;

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public int shootCount = 3;
    public float shootInterval = 0.3f;
    private BossRockAudio _bossAudio; // Cache component audio

    protected override void Awake()
    {
        base.Awake();
        _bossAudio = GetComponent<BossRockAudio>();
    }

    protected override void RegisterBossSkills()
    {
        bossSkills.Clear();

        // Di chuyển: Phase 1 weight = 30, Phase 2 weight = 20
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 30, 20));

        // Skill 1 (LaserSkill): Phase 1 weight = 40, Phase 2 weight = 30
        bossSkills.Add(new SkillWeight(LaserSkill, 40, 30));

        // Skill 2 (ShootSkill): Phase 1 weight = 30, Phase 2 weight = 50
        bossSkills.Add(new SkillWeight(ShootSkill, 30, 50));
    }

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

        // Đảm bảo ngắt âm thanh Laser nếu Boss bị tiêu diệt hoặc tắt effect
        if (_bossAudio != null)
        {
            _bossAudio.StopLaserLoopSound();
        }
    }

    //--------------------------------
    // LASER SKILL
    //--------------------------------
    private IEnumerator LaserSkill()
    {
        canDodgeDuringSkill = false; // <--- TẮT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
        if (bossHeath != null) bossHeath.isInvincible = true; // Bất tử trong khi bắn Laser
        LaserLookAtPlayer tracker = laserObject != null ? laserObject.GetComponent<LaserLookAtPlayer>() : null;
        if (tracker == null) tracker = GetComponentInChildren<LaserLookAtPlayer>();
        try
        {
            for (int i = 0; i < laserCastCount; i++)
            {
                SpitFire();

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

                if (aimLaserLine != null) aimLaserLine.SetActive(false);

                if (!lostTarget)
                {
                    if (laserObject != null) laserObject.SetActive(true);

                    // GỌI ÂM THANH: Bắt đầu tiếng Laser chiếu quét (Loop)
                    if (_bossAudio != null)
                    {
                        _bossAudio.StartLaserLoopSound();
                    }

                    yield return new WaitForSeconds(laserDuration);
                    if (laserObject != null) laserObject.SetActive(false);

                    // GỌI ÂM THANH: Dừng tiếng Laser sau khi bắn xong
                    if (_bossAudio != null)
                    {
                        _bossAudio.StopLaserLoopSound();
                    }
                }
                else
                {
                    Debug.Log("Player đã chui vào góc khuất! Boss Rock hủy bắn Laser.");
                }

                yield return new WaitForSeconds(laserInterval);
            }
        }
        finally
        {
            // BẢO HIỂM: Luôn tắt âm thanh Laser nếu Skill kết thúc đột ngột
            if (_bossAudio != null)
            {
                _bossAudio.StopLaserLoopSound();
            }
            if (bossHeath != null) bossHeath.isInvincible = false; // Hủy bất tử sau khi bắn Laser xong
        }
    }

    //--------------------------------
    // SHOOT SKILL
    //--------------------------------
    private IEnumerator ShootSkill()
    {
        canDodgeDuringSkill = true; // <--- BẬT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
        for (int i = 0; i < shootCount; i++)
        {
            Shoot();

            yield return new WaitForSeconds(0.3f);

            if (target != null && shootPoint != null)
            {
                Vector2 dir = (target.position - shootPoint.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // GỌI ÂM THANH: Phát tiếng bắn đạn đá
                if (_bossAudio != null)
                {
                    _bossAudio.PlayRockShootSound(shootPoint.position);
                }

                Instantiate(bulletPrefab, shootPoint.position, Quaternion.Euler(0, 0, angle));
            }

            yield return new WaitForSeconds(shootInterval);
        }
    }
}