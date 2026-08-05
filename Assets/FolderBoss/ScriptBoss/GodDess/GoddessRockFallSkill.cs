using UnityEngine;
using System.Collections;

public class GoddessRockFallSkill : MonoBehaviour
{
    [Header("Rock Prefab Settings")]
    public GameObject rockPrefab;         // Prefab đá rơi (chứa RockFall.cs)

    [Header("Skill Parameters")]
    public int rockCount = 3;             // Số lượng đá rơi mỗi lần cast
    public float delayBetweenRocks = 0.4f; // Thời gian giữa mỗi lần sinh đá đuổi theo Player

    [Header("Spawn Position Offset")]
    public float spawnOffsetY = 0f;       // Độ lệch Y nếu cần chỉnh độ cao sinh ra đá

    private Transform player;
    private BossEndController controller;

    void Awake()
    {
        controller = GetComponent<BossEndController>();

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
        {
            player = p.transform;
        }
    }

    public IEnumerator Cast()
    {
        if (rockPrefab == null || player == null)
            yield break;

        if (controller != null)
            controller.PlayAttack();

        for (int i = 0; i < rockCount; i++)
        {
            // 1. Lấy trực tiếp vị trí tức thời Player vừa đứng
            Vector3 targetPosition = player.position;

            // 2. Tạo đá rơi ngay lập tức tại điểm đó
            SpawnRockImmediately(targetPosition);

            // 3. Chờ một khoảng rồi mới tiếp tục ghim vị trí mới của Player
            yield return new WaitForSeconds(delayBetweenRocks);
        }

        if (controller != null)
            controller.PlayIdle();
    }

    private void SpawnRockImmediately(Vector3 targetPos)
    {
        Vector3 spawnPos = new Vector3(targetPos.x, targetPos.y + spawnOffsetY, targetPos.z);
        Instantiate(rockPrefab, spawnPos, Quaternion.identity);
    }
}