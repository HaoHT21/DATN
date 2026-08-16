using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SurvivalRoomManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;

    [Header("UI Configurations")]
    [Tooltip("UI Panel chứa Text đếm ngược")]
    public GameObject timerUIPanel;
    [Tooltip("Text hiển thị thời gian còn lại (01:00)")]
    public TextMeshProUGUI timerText;

    [Space(5)]
    [Tooltip("UI Panel Banner thông báo khi bắt đầu phòng sinh tồn")]
    public GameObject bannerUIPanel;
    [Tooltip("Text Banner (VD: Phòng Sinh Tồn - Hãy sống sót!)")]
    public TextMeshProUGUI bannerText;
    [Tooltip("Thời gian hiển thị Banner thông báo (giây)")]
    public float bannerDisplayDuration = 3f;

    [Header("Survival Rule Settings")]
    [Tooltip("Thời gian người chơi cần sinh tồn (giây)")]
    public float survivalTimeLimit = 60f;
    [Tooltip("Thời gian giãn cách tối đa tự động spawn thêm wave nếu quái chưa chết hết")]
    public float respawnInterval = 5f;
    [Range(0f, 1f)]
    [Tooltip("Tỷ lệ xuất hiện phòng sinh tồn (1.0 = 100%)")]
    public float survivalRoomChance = 0.8f;

    [Header("Dynamic Obstacle Settings")]
    [Tooltip("Prefab vật thể/chướng ngại vật trong phòng")]
    public GameObject obstaclePrefab;
    [Tooltip("Số lượng vật thể spawn trong phòng")]
    public int obstacleCount = 3;
    [Tooltip("Thời gian giãn cách tự động đổi vị trí vật thể (giây)")]
    public float obstacleRelocateInterval = 5f;

    [Header("Player Reference")]
    public string playerTag = "Player";
    private Transform playerTransform;

    private class SurvivalRoomState
    {
        public bool decided = false;
        public bool isSurvivalRoom = false;
        public float timeRemaining = 60f;
        public bool isRunning = false;
        public Coroutine loopRoutine = null;
        public Coroutine obstacleRoutine = null;
        public List<GameObject> activeObstacles = new List<GameObject>();
    }

    private Dictionary<int, SurvivalRoomState> roomStates = new Dictionary<int, SurvivalRoomState>();
    private Coroutine bannerRoutine;

    void Start()
    {
        if (timerUIPanel != null)
        {
            timerUIPanel.SetActive(false);
        }

        if (bannerUIPanel != null)
        {
            bannerUIPanel.SetActive(false);
        }

        FindPlayer();
    }

    void Update()
    {
        if (!Application.isPlaying || mapGenerator == null || mapGenerator.runtimeRooms == null) return;

        if (playerTransform == null)
        {
            FindPlayer();
        }

        RoomData activeSurvivalRoomThisFrame = null;

        foreach (var room in mapGenerator.runtimeRooms)
        {
            // BỎ QUA: Phòng bắt đầu, phòng Boss, phòng có Special Object
            if (room.isStartRoom || room.isBossRoom || room.hasSpecialObject) continue;

            // Khởi tạo state phòng nếu chưa có trong Dictionary
            if (!roomStates.TryGetValue(room.roomID, out SurvivalRoomState state))
            {
                state = new SurvivalRoomState { timeRemaining = survivalTimeLimit };
                roomStates[room.roomID] = state;
            }

            // PHÒNG ĐÃ CLEARED -> Dừng và bỏ qua
            if (room.isCleared)
            {
                if (state.isRunning)
                {
                    StopSurvivalLoop(room, state);
                }
                continue;
            }

            // PHÒNG ĐANG KÍCH HOẠT VÀ PLAYER Ở BÊN TRONG
            if (room.isActivated && room.isPlayerInside)
            {
                // Quyết định 1 lần duy nhất xem phòng này có phải dạng sinh tồn không
                if (!state.decided)
                {
                    state.decided = true;
                    state.isSurvivalRoom = Random.value <= survivalRoomChance;
                    state.timeRemaining = survivalTimeLimit;
                }

                if (state.isSurvivalRoom)
                {
                    activeSurvivalRoomThisFrame = room;

                    // Bắt đầu Vòng lặp Infinite Wave & Obstacle nếu chưa chạy
                    if (!state.isRunning)
                    {
                        StopRoomManagerRoutine(room);

                        state.isRunning = true;
                        state.loopRoutine = StartCoroutine(InfiniteWaveRoutine(room, state));
                        state.obstacleRoutine = StartCoroutine(DynamicObstacleRoutine(room, state));

                        // Hiển thị Banner thông báo
                        ShowSurvivalBanner("Cố gắn sống đến hết thời gian");
                    }

                    // Đếm ngược thời gian sinh tồn
                    state.timeRemaining -= Time.deltaTime;

                    // CHIẾN THẮNG: Sinh tồn đủ 60 giây
                    if (state.timeRemaining <= 0f)
                    {
                        state.timeRemaining = 0f;
                        CompleteSurvivalRoom(room, state);
                    }
                }
            }
            else
            {
                // Player thoát khỏi phòng hoặc chết -> Reset lại từ đầu
                if (state.isRunning || room.isActivated)
                {
                    ResetSurvivalRoom(room, state);
                }
            }
        }

        // Cập nhật UI Canvas đếm ngược
        UpdateTimerUI(activeSurvivalRoomThisFrame);
    }

    /// <summary>
    /// Coroutine quản lý việc spawn và tự động dịch chuyển Obstacle mỗi 5 giây
    /// </summary>
    private IEnumerator DynamicObstacleRoutine(RoomData room, SurvivalRoomState state)
    {
        if (obstaclePrefab == null || obstacleCount <= 0) yield break;

        // Khởi tạo danh sách GameObject Obstacle lần đầu
        for (int i = 0; i < obstacleCount; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPos(room);
            GameObject obs = Instantiate(obstaclePrefab, spawnPos, Quaternion.identity);
            state.activeObstacles.Add(obs);
        }

        // Lặp vô hạn để thay đổi vị trí mỗi 5 giây
        while (state.isRunning && state.timeRemaining > 0f)
        {
            yield return new WaitForSeconds(obstacleRelocateInterval);

            if (!state.isRunning) yield break;

            // Đổi vị trí tất cả các Obstacle hiện có
            foreach (GameObject obs in state.activeObstacles)
            {
                if (obs != null)
                {
                    obs.transform.position = GetRandomSpawnPos(room);
                }
            }
        }
    }

    private void ShowSurvivalBanner(string message)
    {
        if (bannerUIPanel == null) return;

        if (bannerText != null)
        {
            bannerText.text = message;
        }

        if (bannerRoutine != null)
        {
            StopCoroutine(bannerRoutine);
        }

        bannerRoutine = StartCoroutine(BannerDisplayRoutine());
    }

    private IEnumerator BannerDisplayRoutine()
    {
        bannerUIPanel.SetActive(true);
        yield return new WaitForSeconds(bannerDisplayDuration);
        bannerUIPanel.SetActive(false);
    }

    private void StopRoomManagerRoutine(RoomData room)
    {
        if (mapGenerator != null && mapGenerator.roomManager != null)
        {
            mapGenerator.roomManager.ResetSingleRoomPublic(room);
            room.isActivated = true;
            mapGenerator.roomManager.SendMessage("FindAndCloseDoors", room, SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator InfiniteWaveRoutine(RoomData room, SurvivalRoomState state)
    {
        int settingIndex = room.roomID - 1;
        if (mapGenerator.roomManager == null || settingIndex < 0 || settingIndex >= mapGenerator.roomManager.roomSettings.Count)
        {
            yield break;
        }

        var roomWaveSetting = mapGenerator.roomManager.roomSettings[settingIndex];
        if (roomWaveSetting.waves == null || roomWaveSetting.waves.Count == 0)
        {
            yield break;
        }

        RoomManager.WaveDataEnemy baseWave = roomWaveSetting.waves[0];

        while (state.isRunning && state.timeRemaining > 0f)
        {
            SpawnWaveEnemies(room, baseWave);

            float timer = 0f;
            while (timer < respawnInterval && room.aliveEnemies.Count > 0 && state.isRunning)
            {
                room.aliveEnemies.RemoveAll(enemy => enemy == null);

                if (room.aliveEnemies.Count == 0) break;

                timer += Time.deltaTime;
                yield return null;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void SpawnWaveEnemies(RoomData room, RoomManager.WaveDataEnemy wave)
    {
        if (wave.enemyPrefabs == null || wave.enemyPrefabs.Count == 0) return;

        for (int i = 0; i < wave.enemyCount; i++)
        {
            GameObject prefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Count)];
            Vector3 pos = GetRandomSpawnPos(room);
            GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

            room.aliveEnemies.Add(enemy);
        }
    }

    private Vector3 GetRandomSpawnPos(RoomData room)
    {
        if (room.spawnPositions == null || room.spawnPositions.Count == 0)
        {
            return GridToWorldPosition(room.center.x, room.center.y);
        }

        Vector3Int tile = room.spawnPositions[Random.Range(0, room.spawnPositions.Count)];
        return GridToWorldPosition(tile.x, tile.y);
    }

    private void CompleteSurvivalRoom(RoomData room, SurvivalRoomState state)
    {
        Debug.Log($"<color=green>[SurvivalRoomManager]</color> Sinh tồn thành công ở Phòng {room.roomID}!");

        StopSurvivalLoop(room, state);
        ClearAllEnemiesInRoom(room);
        ClearAllObstaclesInRoom(state);

        room.isCleared = true;
        room.isActivated = false;

        if (mapGenerator.roomManager != null)
        {
            mapGenerator.roomManager.OpenDoorsPublic(room);
        }
    }

    private void ResetSurvivalRoom(RoomData room, SurvivalRoomState state)
    {
        StopSurvivalLoop(room, state);

        state.timeRemaining = survivalTimeLimit;
        state.isRunning = false;

        ClearAllEnemiesInRoom(room);
        ClearAllObstaclesInRoom(state);

        if (mapGenerator.roomManager != null)
        {
            mapGenerator.roomManager.ResetSingleRoomPublic(room);
        }
    }

    private void StopSurvivalLoop(RoomData room, SurvivalRoomState state)
    {
        state.isRunning = false;

        if (state.loopRoutine != null)
        {
            StopCoroutine(state.loopRoutine);
            state.loopRoutine = null;
        }

        if (state.obstacleRoutine != null)
        {
            StopCoroutine(state.obstacleRoutine);
            state.obstacleRoutine = null;
        }
    }

    private void ClearAllEnemiesInRoom(RoomData room)
    {
        if (room.aliveEnemies != null)
        {
            foreach (GameObject enemy in room.aliveEnemies)
            {
                if (enemy != null) Destroy(enemy);
            }
            room.aliveEnemies.Clear();
        }
    }

    private void ClearAllObstaclesInRoom(SurvivalRoomState state)
    {
        if (state.activeObstacles != null)
        {
            foreach (GameObject obs in state.activeObstacles)
            {
                if (obs != null) Destroy(obs);
            }
            state.activeObstacles.Clear();
        }
    }

    private void UpdateTimerUI(RoomData activeRoom)
    {
        if (activeRoom != null && roomStates.TryGetValue(activeRoom.roomID, out SurvivalRoomState state) && state.isRunning)
        {
            if (timerUIPanel != null && !timerUIPanel.activeSelf)
            {
                timerUIPanel.SetActive(true);
            }

            if (timerText != null)
            {
                int seconds = Mathf.CeilToInt(state.timeRemaining);
                int mins = seconds / 60;
                int secs = seconds % 60;
                timerText.text = string.Format("{0:00}:{1:00}", mins, secs);

                timerText.color = (state.timeRemaining <= 10f) ? Color.red : Color.white;
            }
        }
        else
        {
            if (timerUIPanel != null && timerUIPanel.activeSelf)
            {
                timerUIPanel.SetActive(false);
            }
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private Vector3 GridToWorldPosition(int x, int y)
    {
        Vector3Int cellPosition = new Vector3Int(x, y, 0);
        if (mapGenerator != null && mapGenerator.groundTilemaps != null && mapGenerator.groundTilemaps.Count > 0 && mapGenerator.groundTilemaps[0] != null)
        {
            return mapGenerator.groundTilemaps[0].GetCellCenterWorld(cellPosition);
        }
        return new Vector3(x + 0.5f, y + 0.5f, 0f);
    }
}