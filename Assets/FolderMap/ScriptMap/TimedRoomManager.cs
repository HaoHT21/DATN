using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimedRoomManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;

    [Header("UI Configurations")]
    [Tooltip("UI Panel chứa Text đếm ngược")]
    public GameObject timerUIPanel;
    [Tooltip("Text hiển thị thời gian còn lại (01:00)")]
    public TextMeshProUGUI timerText;

    [Header("Timer Rule Settings")]
    [Tooltip("Thời gian giới hạn cho mỗi phòng (giây)")]
    public float roomTimeLimit = 60f;
    [Range(0f, 1f)]
    [Tooltip("Tỷ lệ xuất hiện phòng đếm giờ (1.0 = 100%)")]
    public float timedRoomChance = 0.8f;

    [Header("Player Reference")]
    public string playerTag = "Player";
    private Transform playerTransform;

    private class TimedRoomState
    {
        public bool decided = false;
        public bool isTimedRoom = false;
        public float timeRemaining = 60f;
        public bool isRunning = false;

        // Lưu lại vị trí ô cửa/rào chắn mà Player thực sự bước qua để vào phòng
        public Vector3Int? entryBarrierCell = null;
    }

    private Dictionary<int, TimedRoomState> roomStates = new Dictionary<int, TimedRoomState>();

    void Start()
    {
        if (timerUIPanel != null)
        {
            timerUIPanel.SetActive(false);
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

        RoomData activeTimedRoomThisFrame = null;

        foreach (var room in mapGenerator.runtimeRooms)
        {
            // BỎ QUA: Phòng bắt đầu, phòng Boss, phòng có Special Object
            if (room.isStartRoom || room.isBossRoom || room.hasSpecialObject) continue;

            // Khởi tạo state phòng nếu chưa có
            if (!roomStates.TryGetValue(room.roomID, out TimedRoomState state))
            {
                state = new TimedRoomState { timeRemaining = roomTimeLimit };
                roomStates[room.roomID] = state;
            }

            // NẾU PHÒNG ĐÃ CLEARED -> Dừng timer & reset lại state
            if (room.isCleared)
            {
                state.isRunning = false;
                state.timeRemaining = roomTimeLimit;
                state.entryBarrierCell = null;
                continue;
            }

            // PHÒNG ĐANG KHÓA CHIẾN ĐẤU (Player ở bên trong và room đang Active)
            if (room.isActivated && room.isPlayerInside)
            {
                // Quyết định 1 lần duy nhất xem phòng này có đếm giờ không
                if (!state.decided)
                {
                    state.decided = true;
                    state.isTimedRoom = Random.value <= timedRoomChance;
                    state.timeRemaining = roomTimeLimit;
                }

                // GHI NHỚ CỬA VÀO: Khi phòng vừa kích hoạt chiến đấu, lưu cửa gần Player nhất
                if (state.entryBarrierCell == null && playerTransform != null && room.barrierPositions != null && room.barrierPositions.Count > 0)
                {
                    state.entryBarrierCell = FindClosestBarrierToPlayer(room, playerTransform.position);
                }

                if (state.isTimedRoom)
                {
                    activeTimedRoomThisFrame = room;
                    state.isRunning = true;

                    // Đếm ngược thời gian
                    state.timeRemaining -= Time.deltaTime;

                    // HẾT GIỜ -> Reset phòng & Đẩy Player về đúng cửa vào
                    if (state.timeRemaining <= 0f)
                    {
                        state.timeRemaining = 0f;
                        ResetAndRepositionPlayer(room, state);
                    }
                }
            }
            else
            {
                // Rời khỏi phòng hoặc chưa kích hoạt -> Tắt đếm giờ
                state.isRunning = false;

                // Nếu phòng không active (ví dụ do reset), reset lại cửa vào đã lưu
                if (!room.isActivated)
                {
                    state.entryBarrierCell = null;
                }
            }
        }

        // Cập nhật hiển thị UI Canvas
        UpdateTimerUI(activeTimedRoomThisFrame);
    }

    /// <summary>
    /// Xử lý hết giờ: Reset phòng và đẩy Player về lại lối vào vừa đi qua
    /// </summary>
    private void ResetAndRepositionPlayer(RoomData room, TimedRoomState state)
    {
        Debug.LogWarning($"[TimedRoomManager] Hết giờ ở Phòng {room.roomID}! Đang tiến hành đẩy Player về cửa vào và Reset...");

        // 1. Dịch chuyển Player về vị trí an toàn tại ĐÚNG CỬA ĐÃ VÀO trước khi gọi Reset
        if (playerTransform != null)
        {
            Vector3 safeResetPos = GetSafeResetPositionForEntry(room, state);
            playerTransform.position = safeResetPos;

            // Triệt tiêu gia tốc vật lý
            Rigidbody2D rb2d = playerTransform.GetComponent<Rigidbody2D>();
            if (rb2d != null) rb2d.linearVelocity = Vector2.zero;
        }

        // 2. Nhờ RoomManager Reset sạch sẽ (Dừng Coroutine, xóa quái, mở cửa)
        if (mapGenerator != null && mapGenerator.roomManager != null)
        {
            mapGenerator.roomManager.ResetSingleRoomPublic(room);
        }

        // 3. Reset đồng hồ & cửa vào về trạng thái ban đầu
        state.timeRemaining = roomTimeLimit;
        state.isRunning = false;
        state.entryBarrierCell = null;
    }

    /// <summary>
    /// Tìm ô rào chắn (Barrier Cell) gần nhất với vị trí hiện tại của Player
    /// </summary>
    private Vector3Int FindClosestBarrierToPlayer(RoomData room, Vector3 pPos)
    {
        Vector3Int closestCell = room.barrierPositions[0];
        float minDistance = float.MaxValue;

        foreach (var cell in room.barrierPositions)
        {
            Vector3 cellWorldPos = GridToWorldPosition(cell.x, cell.y);
            float dist = Vector3.SqrMagnitude(pPos - cellWorldPos);

            if (dist < minDistance)
            {
                minDistance = dist;
                closestCell = cell;
            }
        }

        return closestCell;
    }

    /// <summary>
    /// Tính toán điểm lùi an toàn bên ngoài ĐÚNG cửa Player đã đi vào
    /// </summary>
    private Vector3 GetSafeResetPositionForEntry(RoomData room, TimedRoomState state)
    {
        // 1. Ưu tiên cửa đi vào đã được ghi nhớ lúc kích hoạt phòng
        Vector3Int targetBarrierCell;
        if (state.entryBarrierCell.HasValue)
        {
            targetBarrierCell = state.entryBarrierCell.Value;
        }
        else if (room.barrierPositions != null && room.barrierPositions.Count > 0)
        {
            targetBarrierCell = room.barrierPositions[0];
        }
        else
        {
            // Fallback nếu không có rào chắn
            return GridToWorldPosition(room.bounds.x - 1, room.bounds.y - 1);
        }

        Vector3 barrierWorldPos = GridToWorldPosition(targetBarrierCell.x, targetBarrierCell.y);

        // 2. Tính hướng đẩy: Từ Tâm phòng -> Cửa rào chắn để đẩy Player ra hành lang bên ngoài
        Vector3 roomCenterWorld = GridToWorldPosition(room.center.x, room.center.y);
        Vector3 pushDirection = (barrierWorldPos - roomCenterWorld).normalized;

        // 3. Đẩy Player ra ngoài cửa 1.5 đến 2.0 ô tile
        return barrierWorldPos + pushDirection * 1.8f;
    }

    private void UpdateTimerUI(RoomData activeRoom)
    {
        if (activeRoom != null && roomStates.TryGetValue(activeRoom.roomID, out TimedRoomState state) && state.isRunning)
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