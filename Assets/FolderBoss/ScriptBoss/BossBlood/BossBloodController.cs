using UnityEngine;
using System.Collections;

public class BossBloodController : BossController
{
    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int bulletCount = 2;
    public float bulletInterval = 0.5f;

    [Header("Summon")]
    public GameObject summonPrefab;
    public Transform summonPoint;
    public int summonCount = 3;
    public float summonLifeTime = 3f;

    private BossBloodAudio _bossAudio; // Cache component audio

    protected override void Awake()
    {
        base.Awake();
        // Cache component BossBloodAudio
        _bossAudio = GetComponent<BossBloodAudio>();
    }

    protected override void RegisterBossSkills()
    {
        bossSkills.Clear();

        // Di chuyển: Phase 1 weight = 30, Phase 2 weight = 20
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 50, 30));

        // Skill 1 (AttackSkill): Phase 1 weight = 45, Phase 2 weight = 30
        bossSkills.Add(new SkillWeight(AttackSkill, 25, 50));

        // Skill 2 (SummonSkill): Phase 1 weight = 25, Phase 2 weight = 50
        bossSkills.Add(new SkillWeight(SummonSkill, 25, 20));
    }

    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            bulletCount += 1;
            summonCount += 2;
            moveSpeed += 5;
        }
    }

    //--------------------------------
    // ATTACK SKILL
    //--------------------------------
    private IEnumerator AttackSkill()
    {
        canDodgeDuringSkill = true; // <--- BẬT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
        for (int i = 0; i < bulletCount; i++)
        {
            PlayAttack();
            yield return new WaitForSeconds(0.3f);

            SpawnBullet();

            yield return new WaitForSeconds(1f);

            SpawnBullet();

            yield return new WaitForSeconds(bulletInterval);
        }

        canDodgeDuringSkill = false; // <--- TẮT QUYỀN NÉ ĐẠN CHO SKILL NÀY!

    }

    private void SpawnBullet()
    {
        if (target == null || firePoint == null) return;

        Vector2 dir = (target.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle));

        // PHÁT ÂM THANH BẮN ĐẠN MÁU TRỰC TIẾP TẠI ĐÂY
        if (_bossAudio != null)
        {
            _bossAudio.PlayBloodShootSound(firePoint.position); // <-- Gọi trực tiếp ở đây
        }
    }

    //--------------------------------
    // SUMMON SKILL
    //--------------------------------
    private IEnumerator SummonSkill()
    {
        canDodgeDuringSkill = false; // <--- TẮT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
        Summon();
        yield return new WaitForSeconds(0.5f);

        Transform origin = summonPoint != null ? summonPoint : transform;

        for (int i = 0; i < summonCount; i++)
        {
            Vector2 randomPos = (Vector2)origin.position + Random.insideUnitCircle * 2f;
            GameObject summon = Instantiate(summonPrefab, randomPos, Quaternion.identity);
            Destroy(summon, summonLifeTime);

            // PHÁT ÂM THANH TRIỆU HỒI TẠI VỊ TRÍ ĐỆ TỬ XUẤT HIỆN
            if (_bossAudio != null)
            {
                _bossAudio.PlaySummonSound(randomPos);
            }
        }
    }
}