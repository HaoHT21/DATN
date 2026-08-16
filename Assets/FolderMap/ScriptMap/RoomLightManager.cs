using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class RoomLightManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;

    [Header("Generator Spawn Settings")]
    public GameObject generatorPrefab;   // Prefab chứa script LightGenerator
    [Range(1, 10)]
    public int generatorsPerRoom = 5;     // Số máy phát điện spawn mỗi phòng

    [Header("Boss Room Generator Settings")]
    [Tooltip("Thời gian tự động xóa sạch cũ và spawn bộ máy mới trong phòng Boss (Liên tục mỗi 5s bất kể Sáng/Tối)")]
    public float bossGeneratorRelocateInterval = 5f;

    [Header("Lighting Settings")]
    public Light2D globalLight;
    public float normalIntensity = 1f;    // Độ sáng bình thường của phòng
    public float darkIntensity = 0.05f;   // Độ sáng khi mất điện hoàn toàn
    public float fadeSpeed = 2f;          // Tốc độ chuyển đổi ánh sáng mượt

    [Header("Timer & UI")]
    public float lightDuration = 10f;     // Thời gian đèn sáng trước khi tắt hoàn toàn
    public float gracePeriodAfterRepair = 3f; // Thời gian duy trì sáng sau khi sửa máy
    public GameObject timerUIPanel;
    public Image timerFillImage;
    public TextMeshProUGUI timerText;

    [Header("Player Settings")]
    public string playerTag = "Player";
    public float darkDetectRadius = 3f;   // Bán kính nhắm địch khi phòng bị tối om
    private Transform playerTransform;
    private Light2D playerSpotLight;
    private LookAtEnemy playerLookAtEnemy; // Tham chiếu tới script LookAtEnemy của Player

    // Quản lý dữ liệu máy phát theo từng RoomID
    private Dictionary<int, List<LightGenerator>> roomGenerators = new Dictionary<int, List<LightGenerator>>();
    private Dictionary<int, float> roomTimerState = new Dictionary<int, float>();

    // Quản lý Coroutine đổi vị trí máy phát điện phòng Boss
    private Dictionary<int, Coroutine> bossRelocateRoutines = new Dictionary<int, Coroutine>();

    private RoomData currentActiveRoom = null;
    private bool isPitchBlack = false;

    private void Start()
    {
        if (timerUIPanel != null) timerUIPanel.SetActive(false);
        if (globalLight != null) globalLight.intensity = normalIntensity;

        FindPlayer();
    }

    private void Update()
    {
        if (!Application.isPlaying || mapGenerator == null || mapGenerator.runtimeRooms == null) return;

        if (playerTransform == null || playerSpotLight == null || playerLookAtEnemy == null) FindPlayer();

        RoomData activeRoomThisFrame = null;

        foreach (var room in mapGenerator.runtimeRooms)
        {
            // BỎ QUA: Bắt đầu và Special Object
            if (room.isStartRoom || room.hasSpecialObject) continue;

            // NẾU PHÒNG ĐÃ CLEARED -> Dọn máy phát & dừng vòng lặp
            if (room.isCleared)
            {
                StopBossRelocateRoutine(room.roomID);
                ClearRoomGenerators(room.roomID);
                continue;
            }

            // PHÒNG ĐANG KÍCH HOẠT VÀ PLAYER ĐANG Ở TRONG
            if (room.isActivated && room.isPlayerInside)
            {
                activeRoomThisFrame = room;

                // 1. Kích hoạt Coroutine làm mới máy phát liên tục mỗi 5s cho Phòng Boss (dù Sáng hay Tối)
                if (room.isBossRoom && !bossRelocateRoutines.ContainsKey(room.roomID))
                {
                    Coroutine routine = StartCoroutine(BossGeneratorRelocateRoutine(room));
                    bossRelocateRoutines[room.roomID] = routine;
                }
                else if (!room.isBossRoom)
                {
                    // Phòng thường: Spawn tĩnh nếu chưa có
                    EnsureGeneratorsSpawned(room);
                }

                // 2. Khởi tạo Timer phòng nếu chưa có
                if (!roomTimerState.ContainsKey(room.roomID))
                {
                    roomTimerState[room.roomID] = lightDuration;
                }

                // 3. Đếm ngược thời gian ánh sáng
                if (roomTimerState[room.roomID] > 0f)
                {
                    roomTimerState[room.roomID] -= Time.deltaTime;
                    if (roomTimerState[room.roomID] <= 0f)
                    {
                        roomTimerState[room.roomID] = 0f;
                        isPitchBlack = true; // Tối om hoàn toàn
                    }
                }
            }
            else
            {
                // Player rời phòng -> Reset lại trạng thái phòng
                if (roomTimerState.ContainsKey(room.roomID) && !room.isCleared)
                {
                    StopBossRelocateRoutine(room.roomID);
                    ResetRoomLightState(room);
                }
            }
        }

        currentActiveRoom = activeRoomThisFrame;

        // Cập nhật hệ thống Ánh sáng, UI và Tầm nhìn Player
        UpdateLightingAndUI();
    }

    /// <summary>
    /// Coroutine Phòng Boss: LIÊN TỤC mỗi 5 giây XÓA SẠCH toàn bộ máy cũ và SPAM MỚI máy phát điện (Cả khi SÁNG lẫn TỐI)
    /// </summary>
    private IEnumerator BossGeneratorRelocateRoutine(RoomData room)
    {
        // Spawn lần đầu tiên khi bước vào phòng
        ClearRoomGenerators(room.roomID);
        EnsureGeneratorsSpawned(room);

        while (room.isActivated && room.isPlayerInside && !room.isCleared)
        {
            yield return new WaitForSeconds(bossGeneratorRelocateInterval);

            if (!room.isActivated || !room.isPlayerInside || room.isCleared) yield break;

            // XÓA SẠCH MÁY CŨ & SPAM MỚI TOÀN BỘ MÁY PHÁT ĐIỆN VỚI SỐ LƯỢNG generatorsPerRoom
            ClearRoomGenerators(room.roomID);
            EnsureGeneratorsSpawned(room);
        }

        StopBossRelocateRoutine(room.roomID);
    }

    private void StopBossRelocateRoutine(int roomID)
    {
        if (bossRelocateRoutines.TryGetValue(roomID, out Coroutine routine))
        {
            if (routine != null) StopCoroutine(routine);
            bossRelocateRoutines.Remove(roomID);
        }
    }

    private void EnsureGeneratorsSpawned(RoomData room)
    {
        if (roomGenerators.ContainsKey(room.roomID) && roomGenerators[room.roomID].Count > 0) return;

        List<LightGenerator> spawnedList = new List<LightGenerator>();

        for (int i = 0; i < generatorsPerRoom; i++)
        {
            Vector3 spawnPos = GetRandomTileWorldPos(room);
            GameObject genObj = Instantiate(generatorPrefab, spawnPos, Quaternion.identity);

            LightGenerator genScript = genObj.GetComponent<LightGenerator>();
            if (genScript != null)
            {
                genScript.manager = this;
                spawnedList.Add(genScript);
            }
        }

        roomGenerators[room.roomID] = spawnedList;
    }

    /// <summary>
    /// Gọi khi Player sửa xong 1 máy phát -> Bật sáng lại đèn phòng
    /// </summary>
    public void OnGeneratorRepaired(LightGenerator generator)
    {
        if (currentActiveRoom == null) return;

        int roomID = currentActiveRoom.roomID;

        // BẬT SÁNG LẠI PHÒNG, TẮT ĐÈN PLAYER & RESET TIMER ĐẾM SÁNG
        isPitchBlack = false;
        roomTimerState[roomID] = lightDuration;

        // Lưu ý: Vòng lặp 5 giây vẫn tiếp tục chạy ngầm đếm giờ để đổi vị trí máy phát
    }

    private void ResetRoomLightState(RoomData room)
    {
        roomTimerState[room.roomID] = lightDuration;

        if (roomGenerators.TryGetValue(room.roomID, out List<LightGenerator> gens))
        {
            foreach (var g in gens)
            {
                if (g != null) g.ResetGenerator();
            }
        }
    }

    private void UpdateLightingAndUI()
    {
        // 1. Global Light Target
        float targetGlobalIntensity = (currentActiveRoom != null && isPitchBlack) ? darkIntensity : normalIntensity;

        // 2. Player Spot Light Target
        float targetPlayerSpotIntensity = (currentActiveRoom != null && isPitchBlack) ? 1f : 0f;

        // Transition mượt
        if (globalLight != null)
        {
            globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, targetGlobalIntensity, fadeSpeed * Time.deltaTime);
        }

        if (playerSpotLight != null)
        {
            playerSpotLight.intensity = Mathf.MoveTowards(playerSpotLight.intensity, targetPlayerSpotIntensity, fadeSpeed * Time.deltaTime);
        }

        // 3. Update LookAtEnemy detectRadius
        if (playerLookAtEnemy != null)
        {
            if (currentActiveRoom != null && isPitchBlack)
            {
                playerLookAtEnemy.SetCustomDetectRadius(darkDetectRadius);
            }
            else
            {
                playerLookAtEnemy.ResetDetectRadius();
            }
        }

        // 4. Update UI Timer
        // 4. Cập nhật UI Timer
        if (currentActiveRoom != null && roomTimerState.TryGetValue(currentActiveRoom.roomID, out float timeRem))
        {
            if (timerUIPanel != null && !timerUIPanel.activeSelf) timerUIPanel.SetActive(true);

            // GIỮ NGUYÊN: fill giảm từ 1 -> 0 (100% về 0%)
            float fillRemaining = timeRem / lightDuration;

            // ĐẢO NGƯỢC: fill tăng từ 0 -> 1 (0% lên 100% theo thời gian đã trôi qua)
            float fillProgress = 1f - (timeRem / lightDuration);

            if (timerFillImage != null)
            {
                // Chọn 1 trong 2 dòng dưới đây tùy nhu cầu hiển thị của bạn:

                // Dòng 1: Nếu muốn thanh UI cắm đầy dần từ Đỉnh/Gốc lên 100%:
                timerFillImage.fillAmount = fillProgress;

                // Dòng 2: Nếu muốn thanh UI rút dần về 0% nhưng bị ngược hướng, 
                // hãy dùng Dòng 1 kết hợp chỉnh Fill Origin = Top/Bottom trên Inspector.
            }

            if (timerText != null)
            {
                int secs = Mathf.CeilToInt(timeRem);
                timerText.text = $"{secs}s";
            }
        }
        else
        {
            if (timerUIPanel != null && timerUIPanel.activeSelf) timerUIPanel.SetActive(false);
            isPitchBlack = false;
        }
    }

    private void ClearRoomGenerators(int roomID)
    {
        if (roomGenerators.TryGetValue(roomID, out List<LightGenerator> gens))
        {
            foreach (var g in gens)
            {
                if (g != null) Destroy(g.gameObject);
            }
            roomGenerators.Remove(roomID);
        }
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerSpotLight = playerObj.GetComponentInChildren<Light2D>();
            playerLookAtEnemy = playerObj.GetComponentInChildren<LookAtEnemy>();

            if (playerSpotLight != null)
            {
                playerSpotLight.intensity = 0f;
            }
        }
    }

    private Vector3 GetRandomTileWorldPos(RoomData room)
    {
        if (room.spawnPositions != null && room.spawnPositions.Count > 0)
        {
            Vector3Int tile = room.spawnPositions[Random.Range(0, room.spawnPositions.Count)];
            if (mapGenerator != null && mapGenerator.groundTilemaps != null && mapGenerator.groundTilemaps.Count > 0)
            {
                return mapGenerator.groundTilemaps[0].GetCellCenterWorld(tile);
            }
        }
        return new Vector3(room.center.x + 0.5f, room.center.y + 0.5f, 0f);
    }
}