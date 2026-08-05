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
    public float retreatSpeed = 8f; // Tăng tốc độ rút lui mượt mà hơn

    [Header("Wall Check")]
    public LayerMask obstacleMask; // Chọn Layer chứa Tường (ví dụ: Wall, Obstacle, Ground)
    public float wallCheckDistance = 0.6f; // Khoảng cách nhận diện tường khi đang húc

    Vector2 chargeDirection;
    private BossMinoAudio _bossAudio; // Cache component audio

    protected override void Awake()
    {
        base.Awake();
        _bossAudio = GetComponent<BossMinoAudio>();
    }

    //--------------------------------
    // SETUP SKILLS
    //--------------------------------
    protected override void RegisterBossSkills()
    {
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 40, 30));
        bossSkills.Add(new SkillWeight(ShootSkill, 35, 30));
        bossSkills.Add(new SkillWeight(RedBullSkill, 25, 40));
    }

    //--------------------------------
    // PHASE & EFFECTS
    //--------------------------------
    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            bulletCount += 3;
            shootCount += 2;
            chargeDuration -= 0.2f;
            redBullCount += 1;
            moveSpeed += 5;
        }
    }

    protected override void DisableEffects()
    {
        if (redBullEffect != null)
        {
            redBullEffect.SetActive(false);
        }

        // Tắt âm thanh húc lặp nếu Boss bị hủy giữa chừng
        if (_bossAudio != null)
        {
            _bossAudio.StopChargeLoopSound();
        }

    }
    //--------------------------------
    // SHOOT
    //--------------------------------
    IEnumerator ShootSkill()
    {
        canDodgeDuringSkill = true; // <--- BẬT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
        for (int i = 0; i < shootCount; i++)
        {
            PlayAttack();
            yield return new WaitForSeconds(.3f);

            FireBullets();
            yield return new WaitForSeconds(.5f);

            FireBullets();
            yield return new WaitForSeconds(shootInterval);
        }
        canDodgeDuringSkill = false; // <--- TẮT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
    }

    void FireBullets()
    {
        float spread = 50f;

        // GỌI ÂM THANH BẮN ĐẠN
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
    // REDBULL
    //--------------------------------
    IEnumerator RedBullSkill()
    {
        canDodgeDuringSkill = false; // <--- TẮT QUYỀN NÉ ĐẠN CHO SKILL NÀY!
        if (bossHeath != null) bossHeath.isInvincible = true; // Bất tử trong khi húc
        try
        {
            for (int i = 0; i < redBullCount; i++)
            {
                if (target == null) break;

                chargeDirection = (target.position - transform.position).normalized;
                RedBull();

                // GỌI ÂM THANH: Tiếng gầm chuẩn bị húc
                if (_bossAudio != null)
                {
                    _bossAudio.PlayChargeRoarSound(transform.position);
                }

                yield return new WaitForSeconds(.3f);

                if (redBullEffect != null) redBullEffect.SetActive(true);

                float timer = 0;
                while (timer < chargeDuration)
                {
                    // KIỂM TRA VA CHẠM TƯỜNG KHÔNG CHO ĐỊNH VỊ
                    RaycastHit2D hit = Physics2D.CircleCast(transform.position, 0.4f, chargeDirection, wallCheckDistance, obstacleMask);
                    if (hit.collider != null)
                    {
                        Debug.Log("Boss Mino đã tông vào tường! Hủy lao tới lập tức.");
                        break; // Thoát khỏi vòng lặp lao tới ngay lập tức!
                    }

                    rb.MovePosition(rb.position + chargeDirection * chargeSpeed * Time.deltaTime);
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (redBullEffect != null) redBullEffect.SetActive(false);

                // Ngay lập tức rút lui sau khi húc xong/đụng tường
                yield return RetreatAfterCharge();
                yield return new WaitForSeconds(redBullInterval);
            }
        }
        finally
        {
            if (bossHeath != null) bossHeath.isInvincible = false; // Hủy bất tử sau khi húc xong
        }
    }

    IEnumerator RetreatAfterCharge()
    {
        if (target == null) yield break;

        while (Vector2.Distance(transform.position, target.position) < retreatDistance)
        {
            Vector2 dir = (transform.position - target.position).normalized;
            rb.MovePosition(rb.position + dir * retreatSpeed * Time.deltaTime);
            yield return null;
        }
    }
}