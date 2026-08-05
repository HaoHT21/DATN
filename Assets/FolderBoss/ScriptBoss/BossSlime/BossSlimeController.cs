using UnityEngine;
using System.Collections;

public class BossSlimeController : BossController
{
    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public int bulletCount = 20;
    public int jumpAttackCount = 2;

    [Header("Summon")]
    public GameObject enemyPrefab;

    public int summonCount = 5;
    public float summonRadius = 3f;
    public float summonLifeTime = 10f;

    private BossSlimeAudio _bossAudio; // Cache component audio

    protected override void Awake()
    {
        base.Awake();
        _bossAudio = GetComponent<BossSlimeAudio>();
    }

    //--------------------------------
    // SETUP SKILLS
    //--------------------------------
    protected override void RegisterBossSkills()
    {
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 40, 30));
        bossSkills.Add(new SkillWeight(ShootSkill, 30, 40));
        bossSkills.Add(new SkillWeight(SummonSkill, 30, 30));
    }

    //--------------------------------
    // PHASE
    //--------------------------------
    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            bulletCount += 10;
            summonCount += 2;
            jumpAttackCount += 2;
            moveSpeed += 10;
        }
    }

    //--------------------------------
    // SHOOT (ĐAN XEN GÓC BẮN)
    //--------------------------------
    IEnumerator ShootSkill()
    {
        canDodgeDuringSkill = true; // <--- BẬT QUYỀN NÉ ĐẠN CHO SKILL NÀY!

        float angleStep = 360f / bulletCount;
        for (int wave = 0; wave < jumpAttackCount; wave++)
        {
            PlayAttack();
            yield return new WaitForSeconds(.5f);

            // GỌI ÂM THANH: Phát tiếng nện đất bung đạn
            if (_bossAudio != null)
            {
                Vector3 soundPos = firePoint != null ? firePoint.position : transform.position;
                _bossAudio.PlayJumpAttackSound(soundPos);
            }

            // Đợt chẵn bắn góc chuẩn, đợt lẻ dịch chuyển 1/2 góc bước (bắn vào khe hở)
            float waveOffset = (wave % 2 == 0) ? 0f : (angleStep / 2f);

            for (int i = 0; i < bulletCount; i++)
            {
                float currentAngle = (i * angleStep) + waveOffset;

                Instantiate(
                    bulletPrefab,
                    firePoint.position,
                    Quaternion.Euler(0, 0, currentAngle)
                );
            }

            yield return new WaitForSeconds(.4f);
        }

        canDodgeDuringSkill = false;
    }

    //--------------------------------
    // SUMMON
    //--------------------------------
    IEnumerator SummonSkill()
    {
        canDodgeDuringSkill = true; // <--- BẬT QUYỀN NÉ ĐẠN CHO SKILL NÀY!

        PlayAttack();
        yield return new WaitForSeconds(.5f);

        // GỌI ÂM THANH: Phát tiếng phân rã triệu hồi đàn em
        if (_bossAudio != null)
        {
            _bossAudio.PlaySummonSound(transform.position);
        }

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * summonRadius;
            GameObject enemy = Instantiate(enemyPrefab, pos, Quaternion.identity);
            Destroy(enemy, summonLifeTime);
        }
        canDodgeDuringSkill = false;
    }
}