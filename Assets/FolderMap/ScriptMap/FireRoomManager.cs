using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FireRoomManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;

    [Header("Fire Tilemap & Tile Settings")]
    [Tooltip("Tilemap sẽ được vẽ Tile Lửa/Dung nham lên đó khi Player bước vào phòng")]
    public Tilemap fireTilemap;

    [Tooltip("Danh sách các Tile Lửa (nếu có nhiều hình, sẽ random chọn 1 tile mỗi ô)")]
    public List<TileBase> fireTiles;

    [Header("Spawning Options")]
    [Tooltip("Số lượng Tile Lửa sinh ra mỗi phòng")]
    [Range(1, 100)]
    public int fireTilesPerRoom = 5;

    [Tooltip("Thời gian tự động xóa và vẽ lại Tile Lửa ở vị trí mới (giây)")]
    public float fireRelocateInterval = 5f;

    // Quản lý danh sách Tile Lửa và Coroutine đổi vị trí theo RoomID
    private Dictionary<int, List<Vector3Int>> roomFireTiles = new Dictionary<int, List<Vector3Int>>();
    private Dictionary<int, Coroutine> roomFireRoutines = new Dictionary<int, Coroutine>();

    private void Start()
    {
        if (mapGenerator == null)
        {
            mapGenerator = GetComponent<MapGenerator>();
        }

        ClearAllFireTiles();
    }

    private void Update()
    {
        if (mapGenerator == null || mapGenerator.runtimeRooms == null || fireTilemap == null) return;
        if (fireTiles == null || fireTiles.Count == 0) return;

        foreach (var room in mapGenerator.runtimeRooms)
        {
            // 1. Điều kiện BỎ QUA phòng (Bắt đầu, vật thể đặc biệt, đã cleared)
            if (room.isStartRoom || room.hasSpecialObject || room.isCleared)
            {
                if (roomFireRoutines.ContainsKey(room.roomID))
                {
                    StopAndClearFireForRoom(room.roomID);
                }
                continue;
            }

            // 2. Kiểm tra trạng thái Player trong phòng
            bool isPlayerInThisRoom = room.isActivated && room.isPlayerInside;

            if (isPlayerInThisRoom)
            {
                // Nếu Player MỚI BƯỚC VÀO phòng và chưa chạy chu kỳ vẽ lửa
                if (!roomFireRoutines.ContainsKey(room.roomID))
                {
                    Coroutine routine = StartCoroutine(FireRelocateRoutine(room));
                    roomFireRoutines[room.roomID] = routine;
                }
            }
            else
            {
                // Player ĐÃ RỜI PHÒNG -> Dừng Coroutine và xóa lửa ngay
                if (roomFireRoutines.ContainsKey(room.roomID))
                {
                    StopAndClearFireForRoom(room.roomID);
                }
            }
        }
    }

    /// <summary>
    /// Coroutine liên tục vẽ lửa và tự động đổi vị trí ngẫu nhiên sau mỗi 5 giây
    /// </summary>
    private IEnumerator FireRelocateRoutine(RoomData room)
    {
        while (room.isActivated && room.isPlayerInside && !room.isCleared)
        {
            // Xóa tile lửa cũ trước khi vẽ vị trí mới
            ClearFireTilesInRoom(room.roomID);

            // Vẽ tile lửa mới
            SpawnFireTilesForRoom(room);

            // Chờ 5 giây trước khi thực hiện lần lặp tiếp theo
            yield return new WaitForSeconds(fireRelocateInterval);
        }

        // Tự dọn dẹp khi điều kiện vòng lặp không còn thỏa mãn
        StopAndClearFireForRoom(room.roomID);
    }

    /// <summary>
    /// Tính toán vị trí ngẫu nhiên hoàn toàn trong phòng và vẽ Tile Lửa
    /// </summary>
    private void SpawnFireTilesForRoom(RoomData room)
    {
        RectInt bounds = room.bounds;
        List<Vector3Int> validFloorTiles = new List<Vector3Int>();

        // Duyệt TOÀN BỘ diện tích bounds phòng (Không trừ viền tường)
        for (int x = bounds.x; x < bounds.xMax; x++)
        {
            for (int y = bounds.y; y < bounds.yMax; y++)
            {
                Vector3Int cellPos = new Vector3Int(x, y, 0);

                // 1. Kiểm tra xem ô này có Tile Sàn (Ground) hay không
                bool hasGround = false;
                if (mapGenerator.groundTilemaps != null)
                {
                    foreach (var groundTm in mapGenerator.groundTilemaps)
                    {
                        if (groundTm != null && groundTm.HasTile(cellPos))
                        {
                            hasGround = true;
                            break;
                        }
                    }
                }

                // 2. Kiểm tra xem ô này có bị Vật Cản Cứng (Obstacle) đè lên không
                bool hasObstacle = false;
                if (mapGenerator.obstacleTilemaps != null)
                {
                    foreach (var obsTm in mapGenerator.obstacleTilemaps)
                    {
                        if (obsTm != null && obsTm.HasTile(cellPos))
                        {
                            hasObstacle = true;
                            break;
                        }
                    }
                }

                // Nếu CÓ sàn và KHÔNG CÓ vật cản -> Ô hợp lệ
                if (hasGround && !hasObstacle)
                {
                    validFloorTiles.Add(cellPos);
                }
            }
        }

        if (validFloorTiles.Count == 0)
        {
            Debug.LogWarning($"[FireRoomManager] Room {room.roomID} không tìm thấy Tile Sàn hợp lệ nào!");
            return;
        }

        // Xáo trộn vị trí ngẫu nhiên (Fisher-Yates)
        for (int i = 0; i < validFloorTiles.Count; i++)
        {
            Vector3Int temp = validFloorTiles[i];
            int randomIndex = Random.Range(i, validFloorTiles.Count);
            validFloorTiles[i] = validFloorTiles[randomIndex];
            validFloorTiles[randomIndex] = temp;
        }

        int totalToSpawn = Mathf.Clamp(fireTilesPerRoom, 1, validFloorTiles.Count);
        List<Vector3Int> spawnedPositions = new List<Vector3Int>();

        // Tiến hành vẽ Tile Lửa lên Tilemap
        for (int i = 0; i < totalToSpawn; i++)
        {
            Vector3Int pos = validFloorTiles[i];
            TileBase chosenTile = fireTiles[Random.Range(0, fireTiles.Count)];

            fireTilemap.SetTile(pos, chosenTile);
            spawnedPositions.Add(pos);
        }

        roomFireTiles[room.roomID] = spawnedPositions;
    }

    /// <summary>
    /// Chỉ xóa các Tile Lửa thuộc phòng chỉ định khỏi Tilemap
    /// </summary>
    private void ClearFireTilesInRoom(int roomID)
    {
        if (roomFireTiles.TryGetValue(roomID, out List<Vector3Int> positions))
        {
            foreach (var pos in positions)
            {
                fireTilemap.SetTile(pos, null);
            }
            positions.Clear();
        }
    }

    /// <summary>
    /// Dừng Coroutine đổi vị trí và dọn dẹp sạch Tile Lửa của phòng
    /// </summary>
    private void StopAndClearFireForRoom(int roomID)
    {
        if (roomFireRoutines.TryGetValue(roomID, out Coroutine routine))
        {
            if (routine != null) StopCoroutine(routine);
            roomFireRoutines.Remove(roomID);
        }

        ClearFireTilesInRoom(roomID);
        roomFireTiles.Remove(roomID);
    }

    /// <summary>
    /// Dọn dẹp toàn bộ Tilemap Lửa và dừng mọi Coroutine
    /// </summary>
    public void ClearAllFireTiles()
    {
        StopAllCoroutines();
        roomFireRoutines.Clear();

        if (fireTilemap != null)
        {
            fireTilemap.ClearAllTiles();
        }
        roomFireTiles.Clear();
    }
}