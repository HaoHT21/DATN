using UnityEngine;
using System.Collections;

public class BossAtlantisController : BossController
{
    [Header("Attack")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int bulletCount = 3;
    public float bulletInterval = 0.3f;

    [Header("Fly Skill")]
    public GameObject flyBulletPrefab;
    public int flyBulletCount = 10;
    public float flyDuration = 3f;
    public float flyRadius = 6f;
    public float flySpawnInterval = 0.3f;

    private Collider2D[] allColliders;
    private BossAtlantisAudio _bossAudio; // Cache component audio

    protected override void Awake()
    {
        base.Awake();
        // Lấy tất cả collider trên Boss để quản lý bật/tắt chuẩn xác
        allColliders = GetComponentsInChildren<Collider2D>();

        // Cache component BossAtlantisAudio
        _bossAudio = GetComponent<BossAtlantisAudio>();
    }

    protected override void RegisterBossSkills()
    {
        bossSkills.Clear();

        // Di chuyển
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 40, 30));

        // Skill 1 (AttackSkill)
        bossSkills.Add(new SkillWeight(AttackSkill, 40, 37));

        // Skill 2 (FlySkill)
        bossSkills.Add(new SkillWeight(FlySkill, 20, 33));
    }

    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            bulletCount += 2;
            flyBulletCount += 20;
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

        if (_bossAudio != null)
        {
            _bossAudio.PlayNormalShootSound(firePoint.position); // <-- Gọi trực tiếp ở đây
        }
    }

    //--------------------------------
    // FLY SKILL
    //--------------------------------
    private IEnumerator FlySkill()
    {
        canDodgeDuringSkill = false; // KHÔNG CHO NÉ KHI BAY
        SetCollidersState(false); // Tắt nhận Damage / Va chạm

        Fly(); // Gọi animation bay

        try
        {
            int spawned = 0;
            float timer = 0f;

            while (timer < flyDuration && spawned < flyBulletCount)
            {
                SpawnFlyBullet();
                spawned++;

                yield return new WaitForSeconds(flySpawnInterval);
                timer += flySpawnInterval;
            }
        }
        finally
        {
            SetCollidersState(true); // Bật lại Collider
        }

    }

    private void SpawnFlyBullet()
    {
        if (flyBulletPrefab == null) return;

        Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * flyRadius;
        Instantiate(flyBulletPrefab, pos, Quaternion.identity);

        // Gán kết quả Instantiate vào biến spawnedBullet
        GameObject spawnedBullet = Instantiate(flyBulletPrefab, pos, Quaternion.identity);

        // PHÁT ÂM THANH MƯA ĐẠN TRỰC TIẾP TẠI ĐÂY
        if (_bossAudio != null)
        {
            _bossAudio.PlayFlyShootSound(spawnedBullet);
        }
    }

    private void SetCollidersState(bool state)
    {
        if (allColliders == null) return;
        foreach (var col in allColliders)
        {
            if (col != null) col.enabled = state;
        }
    }

    protected override void DisableEffects()
    {
        base.DisableEffects();
        SetCollidersState(true);
    }
}