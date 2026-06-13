using UnityEngine;
using System.Collections;

public class NPCHealth : MonoBehaviour
{
    [Header("Cấu hình Máu")]
    public float maxHealth = 100f;
    private float currentHealth;

    [Header("Hiệu ứng nổ khi biến hình (VFX)")]
    public GameObject deathEffectPrefab;

    [Header("Prefab của Dạng Kế Tiếp")]
    public GameObject nextPhasePrefab;

    private Animator animator;
    private bool isDead = false;

    // Chuỗi lưu tên chính xác của trạng thái Chết tương ứng với từng dạng
    private string deathStateName = "Death Animation";

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;

        // TỰ ĐỘNG PHÂN TÍCH CONTROLLER ĐỂ LẤY TÊN ANIMATION CHẾT CHÍNH XÁC
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            string controllerName = animator.runtimeAnimatorController.name;

            if (controllerName.Contains("SP_BSS") || controllerName.Contains("Supreme"))
            {
                deathStateName = "SP_Death Animation";  // Khớp ảnh dạng 3
            }
            else if (controllerName.Contains("Dark") || controllerName.Contains("D_BSS"))
            {
                deathStateName = "DarkDeath Animation"; // Khớp ảnh dạng 2
            }
            else
            {
                deathStateName = "Death Animation";     // Khớp ảnh dạng 1
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            StartCoroutine(HandleDeathSequence());
        }
    }

    IEnumerator HandleDeathSequence()
    {
        isDead = true;

        // 1. Tắt toàn bộ AI di chuyển và chiến đấu ngay lập tức
        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != this) script.enabled = false;
        }

        // Khóa luôn vật lý va chạm để Player không bị vướng khi quái đang gục
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // 2. ÉP PHÁT ANIMATION CHẾT THEO TÊN TRỰC TIẾP
        if (animator != null)
        {
            animator.Play(deathStateName);

            // Chờ 1 Frame để Animator cập nhật trạng thái sang clip Chết vừa gọi
            yield return new WaitForEndOfFrame();

            // CHỐT CHẶN VÒNG LẶP: Chờ cho đến khi clip Chết chạy xong hoàn toàn
            bool animationFinished = false;
            while (!animationFinished)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

                // Kiểm tra xem tên trạng thái hiện tại có trùng với tên clip Chết đã chỉ định không
                if (stateInfo.IsName(deathStateName))
                {
                    // Tiến trình normalizedTime >= 0.99f tức là clip đã chạy được 99% thời lượng
                    if (stateInfo.normalizedTime >= 0.99f)
                    {
                        animationFinished = true;
                    }
                }
                else
                {
                    // Dự phòng: Nếu Animator bị lỗi không nhảy vào được clip Chết, ép thoát sau 1.5 giây tránh treo game
                    yield return new WaitForSeconds(1.5f);
                    animationFinished = true;
                }

                if (!animationFinished)
                {
                    yield return null; // Chờ frame tiếp theo để quét lại
                }
            }
        }
        else
        {
            // Nếu quái không có Animator, chờ tạm 0.5 giây
            yield return new WaitForSeconds(0.5f);
        }

        // Ẩn SpriteRenderer của quái cũ đi sau khi diễn xong hoạt ảnh chết để chuẩn bị cho hiệu ứng nổ bùng lên công bằng
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        // 3. XUẤT HIỆN VFX BÙNG NỔ VÀ CHỜ CHẠY HẾT EFFECTS
        float effectDuration = 2.0f; // Thời gian chờ mặc định cho hiệu ứng nổ công phá

        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

            // Tự động phân tích hệ thống Hạt (Particle System) để lấy thời gian chạy chính xác của Effect
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                effectDuration = ps.main.duration;
            }
            else
            {
                // Nếu hiệu ứng dùng chuỗi Animation thường, quét thử xem có Animator không
                Animator effectAnim = effect.GetComponent<Animator>();
                if (effectAnim != null)
                {
                    yield return new WaitForEndOfFrame();
                    effectDuration = effectAnim.GetCurrentAnimatorStateInfo(0).length;
                }
            }

            // Hủy bản sao hiệu ứng ngoài Scene sau khi nó chạy xong
            Destroy(effect, effectDuration);
        }

        // ÉP BUỘC ĐỢI CHẠY HẾT SẠCH THỜI GIAN CỦA HIỆU ỨNG NỔ RỒI MỚI DIỄN RA BƯỚC TIẾP THEO
        yield return new WaitForSeconds(effectDuration);

        // 4. SINH PREFAB CỦA DẠNG TIẾP THEO KHI HIỆU ỨNG ĐÃ CHẠY XONG
        if (nextPhasePrefab != null)
        {
            GameObject nextNPC = Instantiate(nextPhasePrefab, transform.position, transform.rotation);
            nextNPC.name = nextPhasePrefab.name;
            Debug.Log($"🌟 [{nextNPC.name}] Thức tỉnh thành công sau khi hiệu ứng nổ kết thúc!");
        }
        else
        {
            Debug.Log($"💀 [{gameObject.name}] Dạng cuối cùng đã bị tiêu diệt hoàn toàn!");
        }

        // 5. Xóa thực thể cũ khỏi Scene
        Destroy(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            TakeDamage(maxHealth);
        }
    }
}