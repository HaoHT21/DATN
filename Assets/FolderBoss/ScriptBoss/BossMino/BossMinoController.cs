using UnityEngine;
using System.Collections;

public class BossMinoController : BossController
{
    [Header("Shoot")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    public int bulletCount = 5;
    public int shootCount = 2;
    public float shootInterval = .5f;

    [Header("RedBull")]
    public GameObject redBullEffect;

    public float chargeSpeed = 10f;
    public float chargeDuration = 2f;

    public int redBullCount = 1;
    public float redBullInterval = .5f;

    public float retreatDistance = 6f;
    public float retreatSpeed = 8f;

    [Header("Wall Check")]
    public LayerMask obstacleMask; // Layer Tường
    public float bossRadius = 0.5f; // Bán kính vòng tròn va chạm của Boss

    Vector2 chargeDirection;
    private BossMinoAudio _bossAudio;

    protected override void Awake()
    {
        base.Awake();
        _bossAudio = GetComponent<BossMinoAudio>();
    }

    protected override void RegisterBossSkills()
    {
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 30, 20));
        bossSkills.Add(new SkillWeight(ShootSkill, 40, 30));
        bossSkills.Add(new SkillWeight(RedBullSkill, 30, 50));
    }

    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            bulletCount += 2;
            shootCount += 1;
            chargeDuration -= 0.2f;
            redBullCount += 1;
            moveSpeed += 5;
        }
    }

    protected override void DisableEffects()
    {
        if (redBullEffect != null) redBullEffect.SetActive(false);
        if (_bossAudio != null) _bossAudio.StopChargeLoopSound();
    }

    IEnumerator ShootSkill()
    {
        canDodgeDuringSkill = true;
        for (int i = 0; i < shootCount; i++)
        {
            PlayAttack();
            yield return new WaitForSeconds(.3f);

            FireBullets();
            yield return new WaitForSeconds(.5f);

            FireBullets();
            yield return new WaitForSeconds(shootInterval);
        }
        canDodgeDuringSkill = false;
    }

    void FireBullets()
    {
        float spread = 50f;

        if (_bossAudio != null && firePoint != null)
        {
            _bossAudio.PlayShootSpreadSound(firePoint.position);
        }

        if (bulletCount <= 1)
        {
            Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            return;
        }

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = -spread / 2f + (spread / (bulletCount - 1)) * i;
            Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, angle);
            Instantiate(bulletPrefab, firePoint.position, rot);
        }
    }

    //--------------------------------
    // REDBULL SKILL (ĐÃ FIX KẸT TƯỜNG)
    //--------------------------------
    IEnumerator RedBullSkill()
    {
        canDodgeDuringSkill = false;
        if (bossHeath != null) bossHeath.isInvincible = true;

        try
        {
            for (int i = 0; i < redBullCount; i++)
            {
                if (target == null) break;

                chargeDirection = (target.position - transform.position).normalized;
                RedBull();

                if (_bossAudio != null)
                {
                    _bossAudio.PlayChargeRoarSound(transform.position);
                }

                yield return new WaitForSeconds(.3f);

                if (redBullEffect != null) redBullEffect.SetActive(true);

                float timer = 0f;
                bool hitWall = false;

                while (timer < chargeDuration)
                {
                    // Tính khoảng cách Boss sẽ di chuyển trong frame vật lý này
                    float moveDistance = chargeSpeed * Time.fixedDeltaTime;

                    // Quét trước đường đi xem có đụng tường không
                    RaycastHit2D hit = Physics2D.CircleCast(rb.position, bossRadius, chargeDirection, moveDistance, obstacleMask);

                    if (hit.collider != null)
                    {
                        // Đặt Boss sát mép tường (trừ đi bán kính của Boss để không dính vào trong)
                        Vector2 safePosition = hit.point + (hit.normal * bossRadius);
                        rb.MovePosition(safePosition);

                        Debug.Log("Boss Mino chạm tường! Đang bật lùi ra...");
                        hitWall = true;
                        break; // Thoát vòng lặp húc
                    }

                    // Nếu đường đi an toàn -> Di chuyển tiếp
                    rb.MovePosition(rb.position + chargeDirection * moveDistance);

                    timer += Time.fixedDeltaTime;
                    yield return new WaitForFixedUpdate(); // Đồng bộ với Physics
                }

                if (redBullEffect != null) redBullEffect.SetActive(false);

                // Nếu chạm tường, đẩy nhẹ lùi ra 0.2 unit để giải phóng collider hoàn toàn
                if (hitWall)
                {
                    rb.MovePosition(rb.position - chargeDirection * 0.2f);
                    yield return new WaitForSeconds(0.05f);
                }

                // Chuyển sang trạng thái rút lui khỏi vị trí va chạm/người chơi
                yield return RetreatAfterCharge();
                yield return new WaitForSeconds(redBullInterval);
            }
        }
        finally
        {
            if (bossHeath != null) bossHeath.isInvincible = false;
        }
    }

    IEnumerator RetreatAfterCharge()
    {
        if (target == null) yield break;

        while (Vector2.Distance(transform.position, target.position) < retreatDistance)
        {
            Vector2 retreatDir = ((Vector2)transform.position - (Vector2)target.position).normalized;
            float moveDistance = retreatSpeed * Time.fixedDeltaTime;

            // Kiểm tra phía sau lưng khi rút lui xem có vướng tường không
            RaycastHit2D hit = Physics2D.CircleCast(rb.position, bossRadius, retreatDir, moveDistance, obstacleMask);
            if (hit.collider != null)
            {
                // Nếu lùi trúng tường khác thì dừng lùi ngay
                break;
            }

            rb.MovePosition(rb.position + retreatDir * moveDistance);
            yield return new WaitForFixedUpdate();
        }
    }
}