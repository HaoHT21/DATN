using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class RoomData
{
    public int roomID;             // ID định danh phòng
    public RectInt bounds;         // Ranh giới phòng
    public Vector2Int center;      // Tâm phòng

    public bool isStartRoom = false;
    public bool isBossRoom = false;
    public bool hasSpecialObject = false; // Miễn trừ quái nếu có bảo vật

    // --- Dữ liệu điều khiển trận đấu chạy Runtime ---
    public bool isActivated = false;
    public bool isCleared = false;
    public bool isPlayerInside = false;
    public int currentWaveIndex = 0;

    [System.NonSerialized]
    public List<GameObject> aliveEnemies = new List<GameObject>();

    public List<Vector3Int> barrierPositions = new List<Vector3Int>();
    public List<Vector3Int> spawnPositions = new List<Vector3Int>();
}