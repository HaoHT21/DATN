using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RoomManager : MonoBehaviour
{
    [Header("Map")]
    public MapGenerator mapGenerator;

    [Header("Barrier")]
    public Tilemap barrierTilemap;
    public TileBase barrierTile;

    [Header("Wave")]
    public List<RoomWave> roomSettings = new List<RoomWave>();
    public List<WaveDataEnemy> waves = new List<WaveDataEnemy>();
    public float nextWaveDelay = 1.5f;

    private Dictionary<int, Coroutine> activeWaveRoutines = new Dictionary<int, Coroutine>();

    [Header("Boss Camera Zoom")]
    public BossCameraZoom bossCameraZoom;

    // Property giúp lấy danh sách phòng an toàn từ MapGenerator
    public List<RoomData> AllRooms
    {
        get
        {
            if (mapGenerator != null) return mapGenerator.runtimeRooms;
            return null;
        }
    }

    void Start()
    {
        if (barrierTilemap != null)
            barrierTilemap.ClearAllTiles();

        activeWaveRoutines.Clear();

        if (mapGenerator != null && mapGenerator.runtimeRooms != null)
        {
            Debug.Log("Tìm thấy dữ liệu phòng thành công! Số lượng phòng: " + mapGenerator.runtimeRooms.Count);

            foreach (var room in mapGenerator.runtimeRooms)
            {
                room.isActivated = false;
                room.isCleared = false;
                room.isPlayerInside = false;
                room.currentWaveIndex = 0;
                if (room.aliveEnemies == null) room.aliveEnemies = new List<GameObject>();
                room.aliveEnemies.Clear();
            }

            if (mapGenerator.spawnedFinalRoomObject != null)
            {
                mapGenerator.spawnedFinalRoomObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Không tìm thấy dữ liệu MapGenerator hoặc danh sách phòng trống!");
        }
    }

    void Update()
    {
        if (mapGenerator == null || barrierTilemap == null || mapGenerator.runtimeRooms == null || mapGenerator.runtimeRooms.Count == 0)
            return;

        if (!Application.isPlaying)
            return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            ResetAllActiveBattles();
            return;
        }

        Vector3Int cell = barrierTilemap.WorldToCell(player.transform.position);

        foreach (var room in mapGenerator.runtimeRooms)
        {
            bool isDeepInside = IsPlayerDeepInsideRoom(room, cell);
            room.isPlayerInside = isDeepInside;

            if (isDeepInside)
            {
                if (!room.isActivated && !room.isCleared && !room.hasSpecialObject)
                {
                    StartRoomBattle(room);
                }

                int settingIndex = room.roomID - 1;
                if (settingIndex >= 0 && settingIndex < roomSettings.Count)
                {
                    RoomWave setting = roomSettings[settingIndex];
                    if (setting.isBossRoom && room.isActivated)
                    {
                        if (AllEnemiesDeadInRoom(room))
                        {
                            room.isCleared = true;
                            room.isActivated = false;
                            OpenDoors(room);

                            if (bossCameraZoom != null)
                            {
                                bossCameraZoom.ResetZoom();
                            }

                            if (mapGenerator.spawnedFinalRoomObject != null)
                            {
                                mapGenerator.spawnedFinalRoomObject.SetActive(true);
                            }
                            Debug.Log("Boss Room Cleared - Special Object Activated!");
                        }
                    }
                }
            }
            else
            {
                if (room.isActivated && !room.isCleared)
                {
                    ResetSingleRoom(room);
                }
            }
        }
    }

    bool IsPlayerDeepInsideRoom(RoomData room, Vector3Int playerCell)
    {
        int padding = 1;
        int minX = room.bounds.x + padding;
        int maxX = room.bounds.xMax - padding;
        int minY = room.bounds.y + padding;
        int maxY = room.bounds.yMax - padding;

        return playerCell.x >= minX && playerCell.x < maxX &&
               playerCell.y >= minY && playerCell.y < maxY;
    }

    public void Initialize(MapGenerator generator)
    {
        mapGenerator = generator;

        if (barrierTilemap != null)
            barrierTilemap.ClearAllTiles();

        activeWaveRoutines.Clear();
    }

    public void SyncRooms()
    {
        if (mapGenerator == null || mapGenerator.runtimeRooms == null)
            return;

        int count = mapGenerator.runtimeRooms.Count;

        while (roomSettings.Count < count)
        {
            roomSettings.Add(new RoomWave());
        }

        while (roomSettings.Count > count)
        {
            roomSettings.RemoveAt(roomSettings.Count - 1);
        }

        for (int i = 0; i < count; i++)
        {
            roomSettings[i].roomID = i + 1;

            if (i == 0)
            {
                roomSettings[i].disableEnemy = true;
                roomSettings[i].isBossRoom = false;
            }

            if (i == count - 1)
            {
                roomSettings[i].disableEnemy = true;
                roomSettings[i].isBossRoom = true;
            }
        }
    }

    void StartRoomBattle(RoomData room)
    {
        int settingIndex = room.roomID - 1;
        if (settingIndex < 0 || settingIndex >= roomSettings.Count) return;

        RoomWave setting = roomSettings[settingIndex];

        room.isActivated = true;
        FindAndCloseDoors(room);

        if (setting.isBossRoom)
        {
            if (bossCameraZoom != null)
            {
                bossCameraZoom.ZoomOutForBoss();
            }
        }

        if (setting.disableEnemy)
        {
            if (setting.isBossRoom && setting.bossPrefab != null)
            {
                if (mapGenerator.spawnedFinalRoomObject != null)
                {
                    mapGenerator.spawnedFinalRoomObject.SetActive(false);
                }

                Vector3Int bossCell = new Vector3Int(room.center.x, room.center.y, 0);
                Vector3 bossWorldPos = barrierTilemap.GetCellCenterWorld(bossCell);

                GameObject boss = Instantiate(setting.bossPrefab, bossWorldPos, Quaternion.identity);
                room.aliveEnemies.Add(boss);
            }
            else
            {
                room.isCleared = true;
                room.isActivated = false;
                OpenDoors(room);
            }

            return;
        }

        Coroutine routine = StartCoroutine(RoomWaveRoutine(room));
        activeWaveRoutines[room.roomID] = routine;
    }

    IEnumerator RoomWaveRoutine(RoomData room)
    {
        room.currentWaveIndex = 0;
        int settingIndex = room.roomID - 1;
        RoomWave roomWave = roomSettings[settingIndex];

        while (room.currentWaveIndex < roomWave.waves.Count)
        {
            WaveDataEnemy wave = roomWave.waves[room.currentWaveIndex];

            SpawnWaveInRoom(room, wave);

            yield return new WaitUntil(() => AllEnemiesDeadInRoom(room));

            room.currentWaveIndex++;

            if (room.currentWaveIndex < roomWave.waves.Count)
                yield return new WaitForSeconds(nextWaveDelay);
        }

        room.isCleared = true;
        room.isActivated = false;

        OpenDoors(room);

        Debug.Log($"Room {room.roomID} Cleared");

        activeWaveRoutines.Remove(room.roomID);
    }

    void SpawnWaveInRoom(RoomData room, WaveDataEnemy wave)
    {
        foreach (GameObject enemy in room.aliveEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        room.aliveEnemies.Clear();

        for (int i = 0; i < wave.enemyCount; i++)
        {
            if (wave.enemyPrefabs.Count == 0)
                return;

            GameObject prefab = wave.enemyPrefabs[Random.Range(0, wave.enemyPrefabs.Count)];
            Vector3 pos = GetRandomSpawnPos(room);
            GameObject enemy = Instantiate(prefab, pos, Quaternion.identity);

            room.aliveEnemies.Add(enemy);
        }
    }

    Vector3 GetRandomSpawnPos(RoomData room)
    {
        if (room.spawnPositions.Count == 0)
        {
            Vector3Int centerTile = new Vector3Int(room.center.x, room.center.y, 0);
            return barrierTilemap != null ? barrierTilemap.GetCellCenterWorld(centerTile) : new Vector3(room.center.x + 0.5f, room.center.y + 0.5f, 0);
        }

        for (int i = 0; i < 20; i++)
        {
            Vector3Int tile = room.spawnPositions[Random.Range(0, room.spawnPositions.Count)];

            Vector3 worldPos = barrierTilemap.GetCellCenterWorld(tile);
            bool occupied = false;

            foreach (GameObject enemy in room.aliveEnemies)
            {
                if (enemy == null)
                    continue;

                if (Vector2.Distance(enemy.transform.position, worldPos) < 1f)
                {
                    occupied = true;
                    break;
                }
            }

            if (!occupied)
                return worldPos;
        }

        Vector3Int fallbackTile = room.spawnPositions[Random.Range(0, room.spawnPositions.Count)];
        return barrierTilemap.GetCellCenterWorld(fallbackTile);
    }

    bool AllEnemiesDeadInRoom(RoomData room)
    {
        room.aliveEnemies.RemoveAll(enemy => enemy == null);
        return room.aliveEnemies.Count == 0;
    }

    void FindAndCloseDoors(RoomData room)
    {
        foreach (var pos in room.barrierPositions)
        {
            barrierTilemap.SetTile(pos, barrierTile);
        }
    }

    void OpenDoors(RoomData room)
    {
        foreach (var pos in room.barrierPositions)
        {
            barrierTilemap.SetTile(pos, null);
        }
    }

    void ResetSingleRoom(RoomData room)
    {
        if (activeWaveRoutines.ContainsKey(room.roomID))
        {
            if (activeWaveRoutines[room.roomID] != null)
                StopCoroutine(activeWaveRoutines[room.roomID]);

            activeWaveRoutines.Remove(room.roomID);
        }

        foreach (GameObject enemy in room.aliveEnemies)
        {
            if (enemy != null)
                Destroy(enemy);
        }

        if (bossCameraZoom != null)
        {
            bossCameraZoom.ResetZoom();
        }

        room.aliveEnemies.Clear();
        room.isActivated = false;
        room.currentWaveIndex = 0;

        OpenDoors(room);
    }

    public void ResetAllActiveBattles()
    {
        foreach (var pair in activeWaveRoutines)
        {
            if (pair.Value != null)
                StopCoroutine(pair.Value);
        }

        activeWaveRoutines.Clear();

        if (mapGenerator != null && mapGenerator.runtimeRooms != null)
        {
            foreach (var room in mapGenerator.runtimeRooms)
            {
                ResetSingleRoom(room);
            }

            if (mapGenerator.spawnedFinalRoomObject != null)
            {
                mapGenerator.spawnedFinalRoomObject.SetActive(true);
            }
        }

        if (bossCameraZoom != null)
        {
            bossCameraZoom.ResetZoom();
        }
    }

    [System.Serializable]
    public class WaveDataEnemy
    {
        public List<GameObject> enemyPrefabs = new List<GameObject>();
        public int enemyCount = 5;
    }

    [System.Serializable]
    public class RoomWave
    {
        [Header("Room")]
        public int roomID;
        public bool disableEnemy;
        public bool isBossRoom;
        public GameObject bossPrefab;
        public List<WaveDataEnemy> waves = new List<WaveDataEnemy>();
    }
}