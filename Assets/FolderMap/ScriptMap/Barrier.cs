using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Barrier : MonoBehaviour
{
    [Header("Barrier")]
    public TilemapRenderer tilemapRenderer;
    public TilemapCollider2D tilemapCollider;

    [Header("Spawn Area")]
    public Tilemap spawnAreaTilemap;

    [Header("Wave System")]
    public List<WaveData> waves = new List<WaveData>();

    [Header("Wave Delay")]
    public float nextWaveDelay = 2f;

    private bool activated;
    private Collider2D currentPlayer;

    private List<GameObject> aliveEnemies =
        new List<GameObject>();

    private int currentWaveIndex = 0;

    private void Start()
    {
        // Ẩn barrier
        if (tilemapRenderer != null)
            tilemapRenderer.enabled = false;

        // Trigger ban đầu
        if (tilemapCollider != null)
            tilemapCollider.isTrigger = true;
    }

    // =========================
    // PLAYER ĐI VÀO
    // =========================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated)
            return;

        if (other.CompareTag("Player"))
        {
            currentPlayer = other;
        }
    }

    // =========================
    // PLAYER ĐI QUA HẲN
    // =========================
    private void OnTriggerExit2D(Collider2D other)
    {
        if (activated)
            return;

        if (other.CompareTag("Player"))
        {
            if (currentPlayer == other)
            {
                activated = true;

                // Đóng barrier
                if (tilemapRenderer != null)
                    tilemapRenderer.enabled = true;

                if (tilemapCollider != null)
                    tilemapCollider.isTrigger = false;

                Debug.Log("Barrier đã đóng!");

                // Bắt đầu wave
                StartCoroutine(StartWaveRoutine());
            }
        }
    }

    // =========================
    // WAVE SYSTEM
    // =========================
    IEnumerator StartWaveRoutine()
    {
        while (currentWaveIndex < waves.Count)
        {
            WaveData wave = waves[currentWaveIndex];

            Debug.Log("Wave " + (currentWaveIndex + 1));

            SpawnWave(wave);

            // Đợi diệt hết quái
            yield return new WaitUntil(() => AllEnemiesDead());

            Debug.Log("Clear Wave " + (currentWaveIndex + 1));

            currentWaveIndex++;

            yield return new WaitForSeconds(nextWaveDelay);
        }

        // Mở barrier khi clear hết wave
        OpenBarrier();
    }

    // =========================
    // SPAWN WAVE
    // =========================
    void SpawnWave(WaveData wave)
    {
        aliveEnemies.Clear();

        for (int i = 0; i < wave.enemyCount; i++)
        {
            // Random enemy
            GameObject randomEnemy =
                wave.enemyPrefabs[
                    Random.Range(
                        0,
                        wave.enemyPrefabs.Count
                    )
                ];

            // Random vị trí
            Vector3 spawnPos =
                GetRandomPositionInTilemap();

            // Spawn
            GameObject enemy =
                Instantiate(
                    randomEnemy,
                    spawnPos,
                    Quaternion.identity
                );

            aliveEnemies.Add(enemy);
        }
    }

    // =========================
    // RANDOM VỊ TRÍ TRONG TILEMAP
    // =========================
    Vector3 GetRandomPositionInTilemap()
    {
        BoundsInt bounds =
            spawnAreaTilemap.cellBounds;

        while (true)
        {
            int x =
                Random.Range(bounds.xMin, bounds.xMax);

            int y =
                Random.Range(bounds.yMin, bounds.yMax);

            Vector3Int randomCell =
                new Vector3Int(x, y, 0);

            // Chỉ spawn trên tile tồn tại
            if (spawnAreaTilemap.HasTile(randomCell))
            {
                return spawnAreaTilemap
                    .GetCellCenterWorld(randomCell);
            }
        }
    }

    // =========================
    // CHECK QUÁI CHẾT
    // =========================
    bool AllEnemiesDead()
    {
        aliveEnemies.RemoveAll(
            enemy => enemy == null
        );

        return aliveEnemies.Count <= 0;
    }

    // =========================
    // MỞ BARRIER
    // =========================
    void OpenBarrier()
    {
        Debug.Log("Đã clear toàn bộ wave!");

        if (tilemapRenderer != null)
            tilemapRenderer.enabled = false;

        if (tilemapCollider != null)
            tilemapCollider.isTrigger = true;

        activated = false;
    }
}

// ===================================
// DATA WAVE
// ===================================
[System.Serializable]
public class WaveData
{
    [Header("Danh sách quái")]
    public List<GameObject> enemyPrefabs =
        new List<GameObject>();

    [Header("Số lượng quái")]
    public int enemyCount = 5;
}