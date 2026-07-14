using UnityEngine;
using System.Collections;

public class BossDecisionTrigger : MonoBehaviour
{
    [Header("Hiệu ứng nổ khi biến hình (VFX)")]
    public GameObject deathEffectPrefab;

    [Header("Cấu hình Prefab sau khi Boss chết")]
    public GameObject executedSpawnPrefab; // Nhân vật mới sinh ra (Ví dụ: Prefab "End")

    [Header("Khác")]
    // Đã đổi kiểu dữ liệu thành NPCTriggerZone để gọi hàm kích hoạt từ xa
    public NPCTriggerZone nextDialogueTriggerZone;
    public string deathAnimationStateName = "Death Animation";

    private bool isTriggered = false;
    void Start()
    {
        BossHeath hp = GetComponent<BossHeath>();

        if (hp != null)
        {
            hp.OnDeath += ActivateDecisionSequence;
        }
    }

    // 1. Kích hoạt chuỗi logic khi Boss hết máu (Được gọi từ BossHealth)
    public void ActivateDecisionSequence()
    {
        if (isTriggered) return;
        StartCoroutine(HandleSequence());
    }

    private IEnumerator HandleSequence()
    {
        isTriggered = true;

        // Tắt va chạm để Boss không bị vướng
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Chạy hoạt ảnh gục ngã
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.Play(deathAnimationStateName);

        // Chờ diễn hoạt ảnh xong (1.5s đúng như code cũ của bạn)
        yield return new WaitForSeconds(1.5f);

        // CHẠY THẲNG LOGIC SINH PREFAB VÀ MỞ THOẠI (Bỏ qua bước gọi UI phán quyết)
        yield return StartCoroutine(FinalizeDeathSequence());
    }

    // Hàm xử lý nổ, tạo nhân vật và kích hoạt Trigger Zone chạy thẳng thoại
    private IEnumerator FinalizeDeathSequence()
    {
        // Hiệu ứng nổ
        if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 1.5f);
        }

        // Ẩn Boss hiện tại
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.enabled = false;

        yield return new WaitForSeconds(0.5f);

        GameObject spawnedObject = null;

        // Sinh nhân vật mới từ Prefab
        if (executedSpawnPrefab != null)
        {
            spawnedObject = Instantiate(executedSpawnPrefab, transform.position, transform.rotation);
            spawnedObject.name = executedSpawnPrefab.name + "_Spawned";
            spawnedObject.SetActive(false); // Ẩn tạm để nhường quyền cho TriggerZone xử lý hiệu ứng hiện hình mượt mà
        }

        // Mở khóa vùng thoại tiếp theo và ép chạy hội thoại lập tức
        if (nextDialogueTriggerZone != null)
        {
            nextDialogueTriggerZone.gameObject.SetActive(true);

            if (spawnedObject != null)
            {
                // Gọi hàm kích hoạt từ xa đã thêm ở NPCTriggerZone để mở hội thoại ngay tại chỗ
                nextDialogueTriggerZone.TriggerDialogueFromBoss(spawnedObject);
            }
        }
        else
        {
            // Dự phòng nếu không gán Trigger Zone, tự bật NPC lên luôn
            if (spawnedObject != null) spawnedObject.SetActive(true);
        }

        // Tự hủy thực thể Boss
        Destroy(gameObject);
    }
}