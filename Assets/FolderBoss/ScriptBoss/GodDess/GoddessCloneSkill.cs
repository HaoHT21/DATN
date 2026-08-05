using UnityEngine;
using System.Collections;

public class GoddessCloneSkill : MonoBehaviour
{
    [Header("Clone")]
    public GameObject clonePrefab;

    public int cloneCount = 3;

    public float cloneLifeTime = 30f;

    [Header("Spawn Area")]
    public float spawnRadius = 5f;

    public LayerMask wallLayer;

    public int maxTry = 10;

    //--------------------------------

    public IEnumerator Cast()
    {
        if (clonePrefab == null)
        {
            Debug.LogWarning("Clone Skill thiếu setup");
            yield break;
        }

        for (int i = 0; i < cloneCount; i++)
        {
            SpawnClone();

            yield return new WaitForSeconds(.2f);
        }
    }

    //--------------------------------

    void SpawnClone()
    {
        Vector2 spawnPos = FindValidPosition();

        GameObject clone = Instantiate(
            clonePrefab,
            spawnPos,
            Quaternion.identity
        );

        // Lấy Component từ Boss gốc (chính GameObject đang chứa Skill này)
        BossHeath masterHealth = GetComponent<BossHeath>();
        GodDessController masterController = GetComponent<GodDessController>();

        // 1. ĐỒNG BỘ MÁU CHO CLONE
        BossHeath cloneHealth = clone.GetComponent<BossHeath>();
        if (cloneHealth != null && masterHealth != null)
        {
            // Tắt UI thanh máu boss của clone để không bị đè UI
            cloneHealth.enableHealthUI = false;

            // Sử dụng hàm InitializeClone đã viết ở BossHeath để truyền máu
            cloneHealth.InitializeClone(masterHealth.currentHeath, masterHealth.maxHeath);
        }

        // 2. ĐỒNG BỘ TRẠNG THÁI & PHASE CHO CLONE
        GodDessController goddess = clone.GetComponent<GodDessController>();
        if (goddess != null)
        {
            // Đồng bộ phase của clone bằng phase hiện tại của Boss gốc
            if (masterController != null)
            {
                goddess.currentPhase = masterController.currentPhase;
            }

            // Đánh dấu clone
            goddess.isClone = true;

            // Clone không tạo clone
            goddess.cloneSkill = null;
        }

        Destroy(clone, cloneLifeTime);
    }

    //--------------------------------

    Vector2 FindValidPosition()
    {
        Vector2 center = transform.position;

        // Thử nhiều lần
        for (int i = 0; i < maxTry; i++)
        {
            Vector2 randomPos = center + Random.insideUnitCircle * spawnRadius;

            // Kiểm tra có tường không
            Collider2D wall = Physics2D.OverlapCircle(
                randomPos,
                .5f,
                wallLayer
            );

            // Kiểm tra đường từ boss tới vị trí spawn
            RaycastHit2D hit = Physics2D.Linecast(
                center,
                randomPos,
                wallLayer
            );

            if (wall == null && hit.collider == null)
            {
                return randomPos;
            }
        }

        // Nếu thử hết vẫn lỗi
        return center;
    }

    //--------------------------------

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}