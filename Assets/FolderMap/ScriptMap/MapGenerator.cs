using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor; // Cần thiết để lưu dữ liệu trong Editor
#endif

public enum MapLayoutType { Linear, Circular }
public enum RoomVisualType { Standard, Overgrown, Ruined }
public enum RoomShapeType { Rectangle, Circle }
public enum ObstacleLayoutType { Scatter, CenterBlock, Cross, Columns }
public enum FinalRoomDirection { TopRight, TopLeft, BottomLeft, BottomRight, Random }

[ExecuteInEditMode]
public class MapGenerator : MonoBehaviour
{
    [Header("Tilemap Layers")]
    public List<Tilemap> groundTilemaps;
    public List<Tilemap> obstacleTilemaps;
    public List<Tilemap> trapTilemaps;

    [Header("Tile Palettes")]
    public TileBase groundTile;
    public TileBase obstacleTile;
    public TileBase trapTile;

    [Header("Tile Palettes - Advanced Style")]
    public RoomVisualType visualStyle = RoomVisualType.Standard;
    public TileBase mossyGroundTile;
    public TileBase crackedGroundTile;

    [Header("Map Dimensions")]
    public int width = 100;
    public int height = 100;

    [Header("Room Configurations")]
    public int totalRooms = 6;
    public int minRoomSize = 8;
    public int maxRoomSize = 14;
    public MapLayoutType layoutType = MapLayoutType.Linear;

    [Header("Room Shapes")]
    public RoomShapeType roomShape = RoomShapeType.Rectangle;

    [Header("Distance & Safety Settings")]
    public int minRoomDistance = 6;
    public int finalRoomSafetyZone = 14;

    [Header("Path Configurations")]
    public int pathWidth = 2;

    [Header("Start Room Configurations")]
    public GameObject startRoomSpecialObjectPrefab;
    /* Đổi sang public để Unity lưu trạng thái tham chiếu tốt hơn */
    public GameObject spawnedStartRoomObject;
    public Transform startRoomCenterTransform;

    [Header("Final Room Configurations")]
    public bool createFinalRoom = true;
    public FinalRoomDirection finalRoomDirection = FinalRoomDirection.Random;
    public int finalRoomWidth = 16;
    public int finalRoomHeight = 16;
    public GameObject finalRoomSpecialObjectPrefab;
    public GameObject spawnedFinalRoomObject;

    [Header("Obstacle & Trap Settings")]
    public ObstacleLayoutType obstacleLayout = ObstacleLayoutType.Scatter;
    [UnityEngine.Range(0, 30)] public int obstaclesPerRoom = 4;
    [UnityEngine.Range(0, 100)] public int trapsPerRoom = 2;

    [Header("Custom Obstacle Size Settings")]
    [Tooltip("Bán kính/Kích thước cạnh của khối CenterBlock vuông rỗng")]
    [UnityEngine.Range(1, 5)] public int centerBlockSize = 2;
    [Tooltip("Độ rộng/Bán kính của 4 cây cột đặt tại 4 góc phòng")]
    [UnityEngine.Range(1, 4)] public int columnRadius = 1;

    [Header("GameObject Spawner (Special)")]
    public GameObject specialObjectA;
    public GameObject specialObjectB;

    [Header("GameObject Spawner (Random Objects)")]
    public GameObject randomObjectPrefab;
    [Tooltip("Số lượng vật thể ngẫu nhiên muốn sinh ra trên toàn map")]
    public int randomObjectCount = 5;

    public Transform objectContainer;
    public RoomManager roomManager;

    // Giữ danh sách này để hiển thị và lưu lại trong Inspector
    public List<RoomData> runtimeRooms = new List<RoomData>();

    private int[,] map;
    private List<RectInt> roomList = new List<RectInt>();

    public int GetMapValue(int x, int y)
    {
        if (map == null || x < 0 || x >= width || y < 0 || y >= height) return -1;
        return map[x, y];
    }

    public bool IsWalkable(int x, int y)
    {
        if (map == null || x < 0 || x >= width || y < 0 || y >= height) return false;
        return map[x, y] == 1;
    }

    private Vector3 GridToWorldPosition(int x, int y)
    {
        Vector3Int cellPosition = new Vector3Int(x, y, 0);
        if (groundTilemaps != null && groundTilemaps.Count > 0 && groundTilemaps[0] != null)
        {
            return groundTilemaps[0].GetCellCenterWorld(cellPosition);
        }
        return new Vector3(x + 0.5f, y + 0.5f, 0);
    }

    public void GenerateMapInEditor()
    {
        if (groundTilemaps == null || groundTilemaps.Count == 0)
        {
            Debug.LogError("Vui lòng kéo ít nhất 1 Tilemap vào list Ground!");
            return;
        }

        ClearMapInEditor();

        map = new int[width, height];
        roomList.Clear();
        runtimeRooms.Clear();

        GenerateRooms();

        if (createFinalRoom && roomList.Count > 0)
            GenerateFinalRoom();

        ConnectRooms();
        InitializeRoomData();
        SpawnSpecialGameObjects();
        SpawnObstaclesAndTraps();
        SpawnRandomObjects();
        RenderMap();

        if (roomManager != null)
        {
            roomManager.Initialize(this);
            roomManager.SyncRooms();
#if UNITY_EDITOR
            EditorUtility.SetDirty(roomManager);
#endif
        }

        // --- ĐÁNH DẤU LƯU DỮ LIỆU CHỐNG MẤT PHÒNG ---
#if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif

        Debug.Log($"Đã tạo mê cung thành công và lưu lại cấu trúc! Tổng số phòng: {runtimeRooms.Count}");
    }

    public void ClearMapInEditor()
    {
        foreach (var tm in groundTilemaps) if (tm != null) tm.ClearAllTiles();
        foreach (var tm in obstacleTilemaps) if (tm != null) tm.ClearAllTiles();
        foreach (var tm in trapTilemaps) if (tm != null) tm.ClearAllTiles();

        Transform container = objectContainer != null ? objectContainer : transform;
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in container)
        {
            if (child != container && child != null) children.Add(child.gameObject);
        }

        children.ForEach(child =>
        {
            if (child != null) DestroyImmediate(child);
        });

        runtimeRooms.Clear();
        roomList.Clear();
        map = null;

        spawnedStartRoomObject = null;
        startRoomCenterTransform = null;
        spawnedFinalRoomObject = null;

#if UNITY_EDITOR
        EditorUtility.SetDirty(gameObject);
#endif
    }

    void GenerateRooms()
    {
        Vector2 mapCenter = new Vector2(width / 2f, height / 2f);
        float baseRadius = Mathf.Min(width, height) * 0.25f;

        for (int i = 0; i < totalRooms; i++)
        {
            int rWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int rHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            int posX = 0, posY = 0;
            bool validPlace = false;
            int attempts = 0;

            while (!validPlace && attempts < 150)
            {
                attempts++;
                if (layoutType == MapLayoutType.Circular)
                {
                    float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                    float randomRadius = baseRadius * Random.Range(0.6f, 1.3f);
                    posX = Mathf.RoundToInt(mapCenter.x + Mathf.Cos(randomAngle) * randomRadius - rWidth / 2f);
                    posY = Mathf.RoundToInt(mapCenter.y + Mathf.Sin(randomAngle) * randomRadius - rHeight / 2f);
                }
                else
                {
                    int segmentWidth = (width - 25) / totalRooms;
                    posX = 12 + (i * segmentWidth) + Random.Range(-3, 3);
                    posY = Random.Range(15, height - rHeight - 15);
                }

                posX = Mathf.Clamp(posX, 5, width - rWidth - 5);
                posY = Mathf.Clamp(posY, 5, height - rHeight - 5);

                RectInt checkRoom = new RectInt(posX, posY, rWidth, rHeight);

                if (!IsTooCloseToOtherRooms(checkRoom, minRoomDistance))
                {
                    roomList.Add(checkRoom);
                    CarveRoom(checkRoom);
                    validPlace = true;
                }
            }
        }
    }

    void GenerateFinalRoom()
    {
        RectInt lastNormalRoom = roomList[roomList.Count - 1];
        int fWidth = finalRoomWidth;
        int fHeight = finalRoomHeight;
        int posX = 0, posY = 0;
        bool validPlace = false;
        int attempts = 0;

        int minOffset = 12;
        int maxOffset = 22;

        while (!validPlace && attempts < 300)
        {
            attempts++;
            FinalRoomDirection currentDir = finalRoomDirection;
            if (currentDir == FinalRoomDirection.Random)
            {
                currentDir = (FinalRoomDirection)Random.Range(0, 4);
            }

            int offsetX = Random.Range(minOffset, maxOffset);
            int offsetY = Random.Range(minOffset, maxOffset);

            switch (currentDir)
            {
                case FinalRoomDirection.TopRight:
                    posX = lastNormalRoom.xMax + offsetX;
                    posY = lastNormalRoom.yMax + offsetY;
                    break;
                case FinalRoomDirection.TopLeft:
                    posX = lastNormalRoom.xMin - fWidth - offsetX;
                    posY = lastNormalRoom.yMax + offsetY;
                    break;
                case FinalRoomDirection.BottomLeft:
                    posX = lastNormalRoom.xMin - fWidth - offsetX;
                    posY = lastNormalRoom.yMin - fHeight - offsetY;
                    break;
                case FinalRoomDirection.BottomRight:
                    posX = lastNormalRoom.xMax + offsetX;
                    posY = lastNormalRoom.yMin - fHeight - offsetY;
                    break;
            }

            posX = Mathf.Clamp(posX, 5, width - fWidth - 5);
            posY = Mathf.Clamp(posY, 5, height - fHeight - 5);

            RectInt finalRoom = new RectInt(posX, posY, fWidth, fHeight);

            if (!IsTooCloseToOtherRooms(finalRoom, finalRoomSafetyZone))
            {
                roomList.Add(finalRoom);
                CarveRoom(finalRoom);
                validPlace = true;
                break;
            }
        }

        if (!validPlace)
        {
            int forcedX = (finalRoomDirection == FinalRoomDirection.TopLeft || finalRoomDirection == FinalRoomDirection.BottomLeft) ? 6 : width - fWidth - 6;
            int forcedY = (finalRoomDirection == FinalRoomDirection.TopRight || finalRoomDirection == FinalRoomDirection.TopLeft) ? height - fHeight - 6 : 6;

            RectInt forcedRoom = new RectInt(forcedX, forcedY, fWidth, fHeight);
            roomList.Add(forcedRoom);
            CarveRoom(forcedRoom);
        }
    }

    bool IsTooCloseToOtherRooms(RectInt targetRoom, int distance)
    {
        foreach (var room in roomList)
        {
            RectInt dangerousZone = new RectInt(room.x - distance, room.y - distance, room.width + (distance * 2), room.height + (distance * 2));
            if (targetRoom.Overlaps(dangerousZone)) return true;
        }
        return false;
    }

    void ConnectRooms()
    {
        for (int i = 0; i < roomList.Count; i++)
        {
            if (i < roomList.Count - 1)
            {
                Vector2Int startCenter = new Vector2Int(roomList[i].x + roomList[i].width / 2, roomList[i].y + roomList[i].height / 2);
                Vector2Int endCenter = new Vector2Int(roomList[i + 1].x + roomList[i + 1].width / 2, roomList[i + 1].y + roomList[i + 1].height / 2);
                CreateCorridor(startCenter, endCenter);
            }
        }
    }

    void CarveRoom(RectInt room)
    {
        if (roomShape == RoomShapeType.Rectangle)
        {
            for (int x = room.x; x < room.xMax; x++)
            {
                for (int y = room.y; y < room.yMax; y++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height) map[x, y] = 1;
                }
            }
        }
        else if (roomShape == RoomShapeType.Circle)
        {
            float centerX = room.x + room.width / 2f;
            float centerY = room.y + room.height / 2f;
            float radius = Mathf.Min(room.width, room.height) / 2f;

            for (int x = room.x; x < room.xMax; x++)
            {
                for (int y = room.y; y < room.yMax; y++)
                {
                    if (x >= 0 && x < width && y >= 0 && y < height)
                    {
                        float dx = x + 0.5f - centerX;
                        float dy = y + 0.5f - centerY;
                        if (dx * dx + dy * dy <= radius * radius)
                        {
                            map[x, y] = 1;
                        }
                    }
                }
            }
        }
    }

    void CreateCorridor(Vector2Int start, Vector2Int end)
    {
        int currentX = start.x;
        while (currentX != end.x)
        {
            CarvePathBrush(currentX, start.y);
            currentX += (end.x > currentX) ? 1 : -1;
        }

        int currentY = start.y;
        while (currentY != end.y)
        {
            CarvePathBrush(end.x, currentY);
            currentY += (end.y > currentY) ? 1 : -1;
        }
    }

    void CarvePathBrush(int centerX, int centerY)
    {
        int halfWidth = pathWidth / 2;
        for (int xOffset = -halfWidth; xOffset <= halfWidth; xOffset++)
        {
            for (int yOffset = -halfWidth; yOffset <= halfWidth; yOffset++)
            {
                int px = centerX + xOffset;
                int py = centerY + yOffset;
                if (px > 0 && px < width - 1 && py > 0 && py < height - 1) map[px, py] = 1;
            }
        }
    }

    void SpawnObstaclesAndTraps()
    {
        for (int i = 1; i < roomList.Count - 1; i++)
        {
            RectInt room = roomList[i];
            RoomData roomData = runtimeRooms[i];
            if (room.width < 6 || room.height < 6) continue;

            int centerX = room.x + room.width / 2;
            int centerY = room.y + room.height / 2;

            switch (obstacleLayout)
            {
                case ObstacleLayoutType.CenterBlock:
                    if (roomData.hasSpecialObject) break;

                    int size = centerBlockSize;
                    for (int x = centerX - size; x <= centerX + size; x++)
                    {
                        for (int y = centerY - size; y <= centerY + size; y++)
                        {
                            if (x >= room.x + 1 && x < room.xMax - 1 && y >= room.y + 1 && y < room.yMax - 1)
                            {
                                if (x == centerX - size || x == centerX + size || y == centerY - size || y == centerY + size)
                                {
                                    // Bỏ qua 3 ô (centerY - 1, centerY, centerY + 1) để tạo lối đi rộng 3 ô
                                    if (y >= centerY - 1 && y <= centerY + 1) continue;

                                    if (map[x, y] == 1) map[x, y] = 2;
                                }
                            }
                        }
                    }
                    break;

                case ObstacleLayoutType.Cross:
                    // Tăng offset viền (ví dụ: +4) để cắt bớt 2 tile ở phía ngoài cùng của mỗi nhánh
                    int outerOffset = 4;

                    // Tăng khoảng cách từ tâm (ví dụ: > 3) để bỏ qua/xóa bớt tile ở phần khối bên trong
                    int innerGap = 3;

                    // 1. Vẽ nhánh ngang (Trục X)
                    for (int x = room.x + outerOffset; x < room.xMax - outerOffset; x++)
                    {
                        // Chỉ đặt obstacle nếu ô đó cách tâm X lớn hơn innerGap (xóa bớt phần khối bên trong)
                        if (map[x, centerY] == 1 && Mathf.Abs(x - centerX) > innerGap)
                        {
                            map[x, centerY] = 2;
                        }
                    }

                    // 2. Vẽ nhánh dọc (Trục Y)
                    for (int y = room.y + outerOffset; y < room.yMax - outerOffset; y++)
                    {
                        // Chỉ đặt obstacle nếu ô đó cách tâm Y lớn hơn innerGap (xóa bớt phần khối bên trong)
                        if (map[centerX, y] == 1 && Mathf.Abs(y - centerY) > innerGap)
                        {
                            map[centerX, y] = 2;
                        }
                    }
                    break;

                case ObstacleLayoutType.Columns:
                    // Cộng thêm 2 ô offset vào khoảng cách lùi từ mép tường
                    int extraWallDistance = 2;

                    // Tăng khoảng cách cách tường lên (ví dụ cũ là 2..4, nay cộng thêm extraWallDistance sẽ thành 4..6)
                    int paddingX = Mathf.Clamp(room.width / 4 + extraWallDistance, 2 + extraWallDistance, 4 + extraWallDistance);
                    int paddingY = Mathf.Clamp(room.height / 4 + extraWallDistance, 2 + extraWallDistance, 4 + extraWallDistance);

                    Vector2Int[] innerCorners = new Vector2Int[]
                    {
                        new Vector2Int(room.x + paddingX, room.y + paddingY),
                        new Vector2Int(room.xMax - 1 - paddingX, room.y + paddingY),
                        new Vector2Int(room.x + paddingX, room.yMax - 1 - paddingY),
                        new Vector2Int(room.xMax - 1 - paddingX, room.yMax - 1 - paddingY)
                    };

                    foreach (var corner in innerCorners)
                    {
                        for (int cx = corner.x - (columnRadius - 1); cx <= corner.x + (columnRadius - 1); cx++)
                        {
                            for (int cy = corner.y - (columnRadius - 1); cy <= corner.y + (columnRadius - 1); cy++)
                            {
                                if (cx >= room.x + 1 && cx < room.xMax - 1 && cy >= room.y + 1 && cy < room.yMax - 1)
                                {
                                    if (Mathf.Abs(cx - centerX) <= 1 && Mathf.Abs(cy - centerY) <= 1) continue;
                                    if (map[cx, cy] == 1) map[cx, cy] = 2;
                                }
                            }
                        }
                    }
                    break;

                case ObstacleLayoutType.Scatter:
                default:
                    int obsCount = 0;
                    for (int k = 0; k < 150; k++)
                    {
                        if (obsCount >= obstaclesPerRoom) break;
                        int rx = Random.Range(room.x + 2, room.xMax - 2);
                        int ry = Random.Range(room.y + 2, room.yMax - 2);

                        if (map[rx, ry] == 1)
                        {
                            map[rx, ry] = 2;
                            obsCount++;

                            int nextX = rx + Random.Range(-1, 2);
                            int nextY = ry + Random.Range(-1, 2);
                            if (nextX > room.x + 1 && nextX < room.xMax - 2 && nextY > room.y + 1 && nextY < room.yMax - 2)
                            {
                                if (map[nextX, nextY] == 1 && obsCount < obstaclesPerRoom)
                                {
                                    map[nextX, nextY] = 2;
                                    obsCount++;
                                }
                            }
                        }
                    }
                    break;
            }

            int trapCount = 0;
            for (int k = 0; k < 100; k++)
            {
                if (trapCount >= trapsPerRoom) break;
                int rx = Random.Range(room.x + 2, room.xMax - 2);
                int ry = Random.Range(room.y + 2, room.yMax - 2);

                if (map[rx, ry] == 1)
                {
                    map[rx, ry] = 3;
                    trapCount++;
                }
            }
        }
    }

    void InitializeRoomData()
    {
        runtimeRooms.Clear();
        for (int i = 0; i < roomList.Count; i++)
        {
            RectInt room = roomList[i];
            RoomData data = new RoomData();
            data.isStartRoom = (i == 0);
            data.roomID = i + 1;
            data.bounds = room;
            data.isBossRoom = (i == roomList.Count - 1);
            data.center = new Vector2Int(room.x + room.width / 2, room.y + room.height / 2);
            data.aliveEnemies = new List<GameObject>();
            data.barrierPositions = new List<Vector3Int>();
            data.spawnPositions = new List<Vector3Int>();

            for (int x = room.x; x < room.xMax; x++)
            {
                CheckAndAddBarrier(x, room.y, data.barrierPositions);
                CheckAndAddBarrier(x, room.yMax - 1, data.barrierPositions);
            }
            for (int y = room.y; y < room.yMax; y++)
            {
                CheckAndAddBarrier(room.x, y, data.barrierPositions);
                CheckAndAddBarrier(room.xMax - 1, y, data.barrierPositions);
            }

            runtimeRooms.Add(data);
        }
    }

    void UpdateSelectableSpawnPositions()
    {
        foreach (var data in runtimeRooms)
        {
            data.spawnPositions.Clear();
            RectInt room = data.bounds;

            int paddingX = 1;
            int paddingY = 1;

            for (int x = room.x + paddingX; x < room.xMax - paddingX; x++)
            {
                for (int y = room.y + paddingY; y < room.yMax - paddingY; y++)
                {
                    if (map[x, y] == 1 && IsSafeSpawnZone(x, y))
                    {
                        Vector3Int potentialPos = new Vector3Int(x, y, 0);
                        bool nearBarrier = false;
                        foreach (var barrier in data.barrierPositions)
                        {
                            if (Vector3Int.Distance(potentialPos, barrier) < 1.5f)
                            {
                                nearBarrier = true;
                                break;
                            }
                        }

                        if (!nearBarrier)
                        {
                            data.spawnPositions.Add(potentialPos);
                        }
                    }
                }
            }
        }
    }

    bool IsSafeSpawnZone(int centerX, int centerY)
    {
        for (int xOffset = -1; xOffset <= 1; xOffset++)
        {
            for (int yOffset = -1; yOffset <= 1; yOffset++)
            {
                int px = centerX + xOffset;
                int py = centerY + yOffset;

                if (px < 0 || px >= width || py < 0 || py >= height) return false;
                if (map[px, py] != 1) return false;
            }
        }
        return true;
    }

    void CheckAndAddBarrier(int x, int y, List<Vector3Int> barrierList)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            if (map[x, y] == 1)
            {
                barrierList.Add(new Vector3Int(x, y, 0));
            }
        }
    }

    void SpawnSpecialGameObjects()
    {
        Transform parentTransform = objectContainer != null ? objectContainer : transform;

        if (runtimeRooms.Count > 0 && startRoomSpecialObjectPrefab != null)
        {
            RoomData startRoom = runtimeRooms[0];
            Vector3 startRoomCenterWorld = GridToWorldPosition(startRoom.center.x, startRoom.center.y);

            spawnedStartRoomObject = Instantiate(startRoomSpecialObjectPrefab, startRoomCenterWorld, Quaternion.identity, parentTransform);
            spawnedStartRoomObject.name = "StartRoom_Special_Object";
            startRoomCenterTransform = spawnedStartRoomObject.transform;

#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(spawnedStartRoomObject, "Spawn Start Room Object");
#endif
        }

        if (runtimeRooms.Count < 3) return;

        List<int> validRoomIndices = new List<int>();
        for (int i = 1; i < runtimeRooms.Count - 1; i++) validRoomIndices.Add(i);

        int indexA = validRoomIndices[Random.Range(0, validRoomIndices.Count)];
        validRoomIndices.Remove(indexA);
        int indexB = validRoomIndices[Random.Range(0, validRoomIndices.Count)];

        if (specialObjectA != null)
        {
            Vector3 posA = GridToWorldPosition(runtimeRooms[indexA].center.x, runtimeRooms[indexA].center.y);
            GameObject objA = Instantiate(specialObjectA, posA, Quaternion.identity, parentTransform);
            objA.name = "Special_Object_A";
            runtimeRooms[indexA].hasSpecialObject = true;
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(objA, "Spawn Object A");
#endif
        }

        if (specialObjectB != null)
        {
            Vector3 posB = GridToWorldPosition(runtimeRooms[indexB].center.x, runtimeRooms[indexB].center.y);
            GameObject objB = Instantiate(specialObjectB, posB, Quaternion.identity, parentTransform);
            objB.name = "Special_Object_B";
            runtimeRooms[indexB].hasSpecialObject = true;
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(objB, "Spawn Object B");
#endif
        }

        if (createFinalRoom && finalRoomSpecialObjectPrefab != null)
        {
            RoomData bossRoom = runtimeRooms[runtimeRooms.Count - 1];
            Vector3 bossRoomCenterWorld = GridToWorldPosition(bossRoom.center.x, bossRoom.center.y);

            spawnedFinalRoomObject = Instantiate(finalRoomSpecialObjectPrefab, bossRoomCenterWorld, Quaternion.identity, parentTransform);
            spawnedFinalRoomObject.name = "FinalRoom_Special_Object";
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(spawnedFinalRoomObject, "Spawn Final Room Object");
#endif
        }
    }

    void SpawnRandomObjects()
    {
        if (randomObjectPrefab == null || randomObjectCount <= 0) return;

        UpdateSelectableSpawnPositions();
        Transform parentTransform = objectContainer != null ? objectContainer : transform;
        List<RoomData> validRoomsForRandomSpawns = new List<RoomData>();

        foreach (var room in runtimeRooms)
        {
            if (!room.isStartRoom && !room.isBossRoom && !room.hasSpecialObject && room.spawnPositions.Count > 0)
            {
                validRoomsForRandomSpawns.Add(room);
            }
        }

        if (validRoomsForRandomSpawns.Count == 0) return;

        for (int i = 0; i < randomObjectCount; i++)
        {
            RoomData targetRoom = validRoomsForRandomSpawns[Random.Range(0, validRoomsForRandomSpawns.Count)];
            int spawnIndex = Random.Range(0, targetRoom.spawnPositions.Count);
            Vector3Int tilePos = targetRoom.spawnPositions[spawnIndex];
            Vector3 spawnWorldPos = GridToWorldPosition(tilePos.x, tilePos.y);

            GameObject randObj = Instantiate(randomObjectPrefab, spawnWorldPos, Quaternion.identity, parentTransform);
            randObj.name = $"Random_Object_{i + 1}";
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(randObj, "Spawn Random Object");
#endif

            targetRoom.spawnPositions.RemoveAt(spawnIndex);
            if (targetRoom.spawnPositions.Count == 0) validRoomsForRandomSpawns.Remove(targetRoom);
            if (validRoomsForRandomSpawns.Count == 0) break;
        }
    }

    void RenderMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (map[x, y] >= 1)
                {
                    TileBase chosenGround = groundTile;
                    if (visualStyle == RoomVisualType.Overgrown && mossyGroundTile != null)
                        chosenGround = mossyGroundTile;
                    else if (visualStyle == RoomVisualType.Ruined && crackedGroundTile != null)
                        chosenGround = crackedGroundTile;

                    foreach (var tm in groundTilemaps) if (tm != null) tm.SetTile(tilePos, chosenGround);

                    if (map[x, y] == 2)
                    {
                        foreach (var tm in obstacleTilemaps) if (tm != null) tm.SetTile(tilePos, obstacleTile);
                    }
                    else if (map[x, y] == 3)
                    {
                        foreach (var tm in trapTilemaps) if (tm != null) tm.SetTile(tilePos, trapTile);
                    }
                }
            }
        }
    }
}