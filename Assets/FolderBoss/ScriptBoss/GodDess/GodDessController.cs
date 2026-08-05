using UnityEngine;
using System.Collections;

public class GodDessController : BossEndController
{
    [Header("Goddess Skills")]
    public GoddessLaserSkill laserSkill;
    public GoddessMeteorSkill meteorSkill;
    public GoddessRockFallSkill rockSkill;
    public GoddessCloneSkill cloneSkill;

    [Header("Phase Visual Settings")]
    [Tooltip("Danh sách các GameObject sẽ BẬT khi bước vào Phase 2 trở đi")]
    public GameObject[] phase2ObjectsToEnable;

    [Tooltip("Danh sách các GameObject sẽ TẮT khi bước vào Phase 2 trở đi (ví dụ visual Phase 1)")]
    public GameObject[] phase2ObjectsToDisable;

    [Header("Clone Settings")]
    public bool isClone;
    private bool hasTriggeredPhase2Visual = false;

    protected override void Start()
    {
        base.Start();

        // Kiểm tra ngay lập tức trạng thái Phase để bật/tắt Visual đúng thời điểm
        CheckVisualState();
    }

    protected override void UpdatePhase()
    {
        // 1. Giữ logic tính toán Phase gốc từ BossEndController
        base.UpdatePhase();

        // 2. Chặn Clone không được phép lên Phase 3 (Khóa tối đa ở Phase 2)
        if (isClone && currentPhase > 2)
        {
            currentPhase = 2;
        }

        // 3. Cập nhật Visual cho cả Boss và Clone khi đạt Phase 2 trở lên
        CheckVisualState();
    }

    /// <summary>
    /// Xử lý bật/tắt GameObject cho Visual Phase 2 (Áp dụng cho cả Boss và Clone)
    /// </summary>
    private void CheckVisualState()
    {
        if (currentPhase >= 2 && !hasTriggeredPhase2Visual)
        {
            hasTriggeredPhase2Visual = true;
            SetPhase2VisualsActive(true);
        }
        else if (currentPhase < 2 && hasTriggeredPhase2Visual)
        {
            hasTriggeredPhase2Visual = false;
            SetPhase2VisualsActive(false);
        }
    }

    private void SetPhase2VisualsActive(bool isPhase2)
    {
        if (phase2ObjectsToEnable != null)
        {
            foreach (var obj in phase2ObjectsToEnable)
            {
                if (obj != null) obj.SetActive(isPhase2);
            }
        }

        if (phase2ObjectsToDisable != null)
        {
            foreach (var obj in phase2ObjectsToDisable)
            {
                if (obj != null) obj.SetActive(!isPhase2);
            }
        }
    }

    //--------------------------------
    // AI LOGIC & SKILLS
    //--------------------------------

    protected override IEnumerator Think()
    {
        isThinking = true;
        PlayIdle();

        yield return new WaitForSeconds(Random.Range(thinkMin, thinkMax));

        float distance = Vector2.Distance(transform.position, target.position);

        if (distance < dangerDistance)
        {
            int action = Random.Range(0, 2);
            switch (action)
            {
                case 0:
                    yield return StartCoroutine(DashBack());
                    break;
                case 1:
                    yield return StartCoroutine(WalkBack());
                    break;
            }
        }
        else
        {
            int action;

            if (isClone)
            {
                // Clone chỉ Random từ action 0 đến 3 (Loại bỏ action 4: UseClone)
                action = Random.Range(0, 4);
            }
            else
            {
                if (currentPhase == 1)
                {
                    action = Random.Range(0, 5);
                }
                else if (currentPhase == 2)
                {
                    int roll = Random.Range(0, 100);
                    if (roll < 10) action = 0;
                    else if (roll < 20) action = 1;
                    else if (roll < 40) action = 2;
                    else if (roll < 50) action = 3;
                    else if (roll < 80) action = 4;
                    else action = 5;
                }
                else // Phase 3 (Chỉ Boss chính mới xuống được dòng này)
                {
                    int roll = Random.Range(0, 100);
                    if (roll < 10) action = 0;
                    else if (roll < 20) action = 1;
                    else if (roll < 40) action = 2;
                    else if (roll < 50) action = 3;
                    else if (roll < 70) action = 4;
                    else action = 5;
                }
            }

            switch (action)
            {
                case 0:
                    yield return StartCoroutine(WalkToPlayer());
                    break;
                case 1:
                    yield return StartCoroutine(CircleMove());
                    break;
                case 2:
                    yield return StartCoroutine(UseLaser());
                    break;
                case 3:
                    yield return StartCoroutine(UseMeteor());
                    break;
                case 4:
                    yield return StartCoroutine(UseRockFall());
                    break;
                case 5:
                    // Bảo vệ thêm 1 lớp kiểm tra: Clone không thể gọi UseClone
                    if (!isClone)
                    {
                        yield return StartCoroutine(UseClone());
                    }
                    break;
            }
        }

        isThinking = false;
    }

    IEnumerator UseLaser()
    {
        usingSkill = true;
        PlayAttack();
        yield return StartCoroutine(laserSkill.Cast());
        usingSkill = false;
    }

    IEnumerator UseMeteor()
    {
        usingSkill = true;
        PlayAttack();
        yield return StartCoroutine(meteorSkill.Cast());
        usingSkill = false;
    }

    IEnumerator UseRockFall()
    {
        usingSkill = true;
        PlayAttack();
        yield return StartCoroutine(rockSkill.Cast());
        usingSkill = false;
    }

    IEnumerator UseClone()
    {
        // Kiểm tra loại bỏ Clone hoàn toàn trước khi thi triển skill Phân Thân
        if (isClone || cloneSkill == null)
            yield break;

        usingSkill = true;

        try
        {
            PlayAttack();
            yield return StartCoroutine(cloneSkill.Cast());
        }
        finally
        {
            usingSkill = false;
        }
    }
}