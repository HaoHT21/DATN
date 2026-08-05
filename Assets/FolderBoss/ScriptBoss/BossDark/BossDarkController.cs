using UnityEngine;
using System.Collections;

public class BossDarkController : BossController
{
    [Header("Cast Skill Settings")]
    public GameObject bulletCastPrefab;
    public Transform castPoint;
    public int castCount = 10;          // Tổng số đạn xả ra trong 1 lần
    public float castRadius = 1.5f;      // Bán kính vòng tròn xung quanh castPoint
    public float castInterval = 0.5f;    // Delay chờ khớp với điểm vung tay của animation Cast

    [Header("Invisible Skill Settings")]
    public float invisibleDuration = 4f;
    public float invisibleMoveSpeed = 20f;
    public float preInvisibleDelay = 0.3f;  // Delay chuẩn bị trước khi biến mất
    public float postInvisibleDelay = 0.4f; // Delay sau khi hiện hình rồi mới tung chiêu

    [Header("Spawn Attack Settings")]
    public GameObject spawnBulletPrefab;
    public Transform spawnPoint;
    public int spawnBulletCount = 12;

    private BoxCollider2D bodyCollider;
    private BossDarkAudio _bossAudio; // Cache component audio

    //--------------------------------
    // SETUP
    //--------------------------------
    protected override void Awake()
    {
        base.Awake(); // Đảm bảo gọi logic Awake của BossController cha
        bodyCollider = GetComponent<BoxCollider2D>();

        if (sprites == null || sprites.Length == 0)
            sprites = GetComponentsInChildren<SpriteRenderer>();

        if (hitColliders == null || hitColliders.Length == 0)
            hitColliders = GetComponentsInChildren<Collider2D>();

        // Cache component BossDarkAudio
        _bossAudio = GetComponent<BossDarkAudio>();
    }

    //--------------------------------
    // SETUP SKILLS
    //--------------------------------
    protected override void RegisterBossSkills()
    {
        bossSkills.Add(new SkillWeight(() => DoMoveBehavior(Random.Range(0.8f, 1.5f)), 40, 30));
        bossSkills.Add(new SkillWeight(CastSkill, 40, 30));
        bossSkills.Add(new SkillWeight(InvisibleSkill, 20, 40));
    }

    //--------------------------------
    // PHASE & HELPER
    //--------------------------------
    protected override void OnPhaseChange(int phase)
    {
        if (phase == 2)
        {
            castCount += 3;
            spawnBulletCount += 10;
            moveSpeed += 10;
        }
    }

    /// <summary>
    /// Ẩn/Hiện hình Boss bao gồm Renderers, Colliders, Animator và VFX Phase 2 của lớp cha
    /// </summary>
    protected void SetInvisible(bool value)
    {
        // 1. Ẩn/Hiện các Sprite Renderer
        foreach (SpriteRenderer sp in sprites)
        {
            if (sp != null) sp.enabled = !value;
        }

        // 2. Ẩn/Hiện các Colliders nhận sát thương / va chạm
        foreach (Collider2D col in hitColliders)
        {
            if (col != null) col.enabled = !value;
        }

        // 3. Ẩn/Hiện VFX Phase 2
        if (phase2VFXObject != null && currentPhase >= 2)
        {
            phase2VFXObject.SetActive(!value);
        }
    }

    //--------------------------------
    // CAST SKILL
    //--------------------------------
    IEnumerator CastSkill()
    {
        canDodgeDuringSkill = true; // Cho phép né đạn trong khi tung chiêu này

        Cast(); // Bật Animation gồng chiêu

        // Đợi một khoảng thời gian castInterval cho khớp với khung hình vung tay của Anim
        yield return new WaitForSeconds(castInterval);

        // Xả HẾT đạn cùng 1 lúc tại vị trí ngẫu nhiên quanh castPoint
        if (bulletCastPrefab != null && castPoint != null)
        {
            for (int i = 0; i < castCount; i++)
            {
                Vector2 randomOffset = Random.insideUnitCircle * castRadius;
                Vector3 spawnPosition = castPoint.position + (Vector3)randomOffset;

                Instantiate(bulletCastPrefab, spawnPosition, castPoint.rotation);
            }

            // PHÁT ÂM THANH XẢ ĐẠN MA THUẬT
            if (_bossAudio != null)
            {
                _bossAudio.PlayCastShootSound(castPoint.position);
            }
        }

        canDodgeDuringSkill = false;
    }

    //--------------------------------
    // INVISIBLE SKILL
    //--------------------------------
    IEnumerator InvisibleSkill()
    {
        canDodgeDuringSkill = false; // Tắt né đạn khi tàng hình

        // --- BƯỚC 1: Delay ngắn trước khi biến mất hoàn toàn ---
        PlayIdle();
        yield return new WaitForSeconds(preInvisibleDelay);

        // --- BƯỚC 2: Tàng hình & Di chuyển ---
        SetInvisible(true);

        // PHÁT ÂM THANH BẮT ĐẦU TÀNG HÌNH
        if (_bossAudio != null)
        {
            _bossAudio.PlayInvisibleEnterSound();
        }

        PickInvisibleTarget();

        float timer = 0f;
        while (timer < invisibleDuration)
        {
            MoveInvisible();
            timer += Time.deltaTime;
            yield return null;
        }

        // --- BƯỚC 3: Hiện hình trở lại ---
        SetInvisible(false);
        rb.linearVelocity = Vector2.zero; // Reset lực di chuyển

        // --- BƯỚC 4: Delay sau khi hiện hình + Chạy Anim báo hiệu ---
        PlayIdle();
        yield return new WaitForSeconds(postInvisibleDelay);

        // --- BƯỚC 5: Xả đạn vòng tròn ---
        SpawnCircleBullet();
    }

    void MoveInvisible()
    {
        //Tăng khoảng cách ngẫu nhiên để di chuyển đến target mới nếu quá gần
        if (Vector2.Distance(transform.position, randomTarget) < 1.2f)
        {
            PickInvisibleTarget();
        }

        Vector2 dir = (randomTarget - (Vector2)transform.position).normalized;

        // Di chuyển Boss tàng hình theo hướng dir với tốc độ invisibleMoveSpeed
        rb.MovePosition(rb.position + dir * invisibleMoveSpeed * Time.deltaTime);
    }

    void PickInvisibleTarget()
    {
        for (int i = 0; i < 10; i++)
        {
            // Mở rộng bán kính tìm điểm khi tàng hình (gấp đôi randomMoveRadius của lớp cha)
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * (randomMoveRadius * 2f);

            if (Physics2D.OverlapCircle(pos, .5f, wallLayer)) continue;

            RaycastHit2D hit = Physics2D.Linecast(transform.position, pos, wallLayer);
            if (hit.collider != null) continue;

            randomTarget = pos;
            return;
        }
        randomTarget = transform.position;
    }

    //--------------------------------
    // BULLET CIRCLE
    //--------------------------------
    void SpawnCircleBullet()
    {
        if (spawnBulletPrefab == null || spawnPoint == null) return;

        for (int i = 0; i < spawnBulletCount; i++)
        {
            float angle = (360f / spawnBulletCount) * i;
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            Instantiate(spawnBulletPrefab, spawnPoint.position, rot);
        }

        // PHÁT ÂM THANH BÙNG NỔ BÃO ĐẠN VÒNG TRÒN
        if (_bossAudio != null)
        {
            _bossAudio.PlayCircleBurstSound(spawnPoint.position);
        }
    }
}