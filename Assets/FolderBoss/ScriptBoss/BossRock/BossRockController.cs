using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BossRockController : BossController
{
    [Header("Laser Setup")]
    public GameObject laserObject;
    public float telegraphDuration = 0.8f;
    public float laserDuration = 2f;
    public int laserCastCount = 1;
    public float laserInterval = 0.5f;
    public List<GameObject> aimLaserLine = new List<GameObject>(); // Danh sách 4 tia line nhắm

    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform shootPoint;
    public int shootCount = 3;
    public float shootInterval = 0.3f;
    public int bulletsPerSpread = 3;     // Số đạn xòe ra mỗi đợt bắn
    public float spreadAngle = 30f;      // Góc xòe của chùm đạn

    private BossRockAudio _bossAudio;

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
            shootCount += 1;
            bulletsPerSpread += 2;
            laserCastCount += 1;
            moveSpeed += 5;
        }
    }

    protected override void DisableEffects()
    {
        if (laserObject != null) laserObject.SetActive(false);

        // Tắt toàn bộ List đường line nhắm
        SetAimLinesActive(false);

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
        canDodgeDuringSkill = false;
        if (bossHeath != null) bossHeath.isInvincible = true;

        LaserLookAtPlayer tracker = laserObject != null ? laserObject.GetComponent<LaserLookAtPlayer>() : null;
        if (tracker == null) tracker = GetComponentInChildren<LaserLookAtPlayer>();

        try
        {
            for (int i = 0; i < laserCastCount; i++)
            {
                SpitFire();

                // Phase ngắm: Bắt đầu khóa vị trí/hướng ngẫu nhiên và BẬT danh sách Line
                if (tracker != null) tracker.StartTelegraph();
                SetAimLinesActive(true);

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

                // TẮT danh sách Line báo hiệu
                SetAimLinesActive(false);

                if (!lostTarget)
                {
                    if (laserObject != null) laserObject.SetActive(true);

                    // Bắt đầu xoay Laser
                    if (tracker != null) tracker.StartLaserRotation(true);

                    if (_bossAudio != null)
                    {
                        _bossAudio.StartLaserLoopSound();
                    }

                    yield return new WaitForSeconds(laserDuration);

                    // Dừng xoay Laser
                    if (tracker != null) tracker.StopLaserRotation();

                    if (laserObject != null) laserObject.SetActive(false);

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
            if (_bossAudio != null)
            {
                _bossAudio.StopLaserLoopSound();
            }
            if (bossHeath != null) bossHeath.isInvincible = false;
        }
    }

    //--------------------------------
    // SHOOT SKILL
    //--------------------------------
    private IEnumerator ShootSkill()
    {
        canDodgeDuringSkill = true;

        for (int i = 0; i < shootCount; i++)
        {
            Shoot();

            yield return new WaitForSeconds(0.3f);

            if (target != null && shootPoint != null)
            {
                Vector2 dir = (target.position - shootPoint.position).normalized;
                float centerAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                if (_bossAudio != null)
                {
                    _bossAudio.PlayRockShootSound(shootPoint.position);
                }

                // Bắn xòe chùm đạn
                FireSpreadBullets(centerAngle);
            }

            yield return new WaitForSeconds(shootInterval);
        }

        canDodgeDuringSkill = false;
    }

    //--------------------------------
    // HELPER METHODS
    //--------------------------------
    private void SetAimLinesActive(bool active)
    {
        if (aimLaserLine == null) return;

        foreach (GameObject line in aimLaserLine)
        {
            if (line != null)
            {
                line.SetActive(active);
            }
        }
    }

    private void FireSpreadBullets(float centerAngle)
    {
        if (bulletsPerSpread <= 1)
        {
            Instantiate(bulletPrefab, shootPoint.position, Quaternion.Euler(0, 0, centerAngle));
            return;
        }

        float startAngle = centerAngle - (spreadAngle / 2f);
        float step = spreadAngle / (bulletsPerSpread - 1);

        for (int b = 0; b < bulletsPerSpread; b++)
        {
            float currentAngle = startAngle + (step * b);
            Instantiate(bulletPrefab, shootPoint.position, Quaternion.Euler(0, 0, currentAngle));
        }
    }
}