using UnityEngine;
using System.Collections;

public class BossIceController : BossController
{
    [Header("Ice Burst")]
    public GameObject icePrefab;
    public Transform icePoint;
    public int iceBurstCount = 1;
    public int iceCount = 20;
    public float iceInterval = 0.3f;

    [Header("Attack Ice")]
    public GameObject attackIcePrefab;
    public Transform attackIcePoint;
    public int attackIceCount = 3;
    public float attackIceInterval = 0.3f;
    private BossIceAudio _bossAudio; // Cache component audio

    protected override void Awake()
    {
        base.Awake();
        _bossAudio = GetComponent<BossIceAudio>(); // Cache component BossIceAudio
    }
    protected override void RegisterBossSkills()
    {
        bossSkills.Clear();

        // Di chuyển: Phase 1 weight = 50, Phase 2 weight = 20
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 40, 30));

        // Skill 1 (IceSkill): Phase 1 weight = 25, Phase 2 weight = 30
        bossSkills.Add(new SkillWeight(IceSkill, 30, 30));

        // Skill 2 (AttackIceSkill): Phase 1 weight = 25, Phase 2 weight = 50
        bossSkills.Add(new SkillWeight(AttackIceSkill, 30, 40));
    }

    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            iceBurstCount += 2;
            iceCount += 10;
            attackIceCount += 2;
            moveSpeed += 5;
        }
    }

    //--------------------------------
    // ICE BURST SKILL (GÓC NGẪU NHIÊN
    //--------------------------------
    private IEnumerator IceSkill()
    {
        canDodgeDuringSkill = true; // <--- BẬT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
        float angleStep = 360f / iceCount;
        for (int wave = 0; wave < iceBurstCount; wave++)
        {
            IceBurst(); // Giữ nguyên trigger animation nếu có
            yield return new WaitForSeconds(iceInterval);

            // GỌI ÂM THANH: Phát 1 lần cho cả đợt bão băng bung ra
            if (_bossAudio != null && icePoint != null)
            {
                _bossAudio.PlayIceBurstSound(icePoint.position);
            }

            // Tạo 1 góc lệch ngẫu nhiên từ 0 đến 360 độ cho cả đợt nhảy này
            float randomWaveOffset = Random.Range(0f, angleStep);

            for (int i = 0; i < iceCount; i++)
            {
                float angle = (i * angleStep) + randomWaveOffset;
                Instantiate(icePrefab, icePoint.position, Quaternion.Euler(0, 0, angle));
            }

            yield return new WaitForSeconds(0.4f);
        }

        canDodgeDuringSkill = false; // <--- TẮT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
    }

    //--------------------------------
    // ATTACK ICE SKILL
    //--------------------------------
    private IEnumerator AttackIceSkill()
    {
        canDodgeDuringSkill = true; // <--- BẬT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
        for (int i = 0; i < attackIceCount; i++)
        {
            PlayAttack();
            yield return new WaitForSeconds(0.3f);

            SpawnAttackIce();

            yield return new WaitForSeconds(attackIceInterval);
        }
        canDodgeDuringSkill = false; // <--- TẮT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
    }

    private void SpawnAttackIce()
    {
        if (target == null || attackIcePoint == null) return;

        Vector2 dir = (target.position - attackIcePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Instantiate(attackIcePrefab, attackIcePoint.position, Quaternion.Euler(0, 0, angle));

        // GỌI ÂM THANH: Phát tiếng phóng gai băng mỗi khi sinh đạn
        if (_bossAudio != null)
        {
            _bossAudio.PlayAttackIceSound(attackIcePoint.position);
        }
    }
}