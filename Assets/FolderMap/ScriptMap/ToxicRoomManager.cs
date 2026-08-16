using System.Collections.Generic;
using UnityEngine;

public class ToxicRoomManager : MonoBehaviour
{
    [Header("References")]
    public MapGenerator mapGenerator;

    [Header("Toxic VFX Prefab")]
    [Tooltip("Prefab chứa Particle System + CircleCollider2D + PoisonArea")]
    public GameObject toxicVFXPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Bán kính tối đa của Collider gây sát thương")]
    public float targetRadius = 5f;

    [Tooltip("Hệ số nhân cho Particle System để bằng kích thước Collider (Ví dụ: VFX 12 / Collider 4 = 3.0)")]
    public float particleScaleMultiplier = 3.0f;

    [Header("Timing Settings")]
    [Tooltip("Thời gian (giây) để khí độc phóng to từ 0 đến bán kính targetRadius")]
    public float growDuration = 15f;
    [Tooltip("Thời gian (giây) để khí độc thu nhỏ về 0 khi dọn xong phòng hoặc player rời đi")]
    public float shrinkDuration = 1.5f;

    public Transform objectContainer;

    private class ToxicRoomState
    {
        public GameObject zoneInstance;
        public ParticleSystem particleSys;
        public CircleCollider2D areaCollider;
        public float currentScale = 0f;
        public bool isFullyGrown = false;
    }

    private Dictionary<int, ToxicRoomState> roomStates = new Dictionary<int, ToxicRoomState>();

    void Update()
    {
        if (!Application.isPlaying || mapGenerator == null || mapGenerator.runtimeRooms == null) return;

        foreach (var room in mapGenerator.runtimeRooms)
        {
            if (room.isStartRoom || room.isBossRoom || room.hasSpecialObject) continue;

            if (!roomStates.TryGetValue(room.roomID, out ToxicRoomState state))
            {
                state = new ToxicRoomState();
                roomStates[room.roomID] = state;
            }

            if (room.isCleared)
            {
                if (state.zoneInstance != null)
                {
                    UpdateVFXShrink(state);
                }
                continue;
            }

            bool isPlayerInRoom = room.isActivated;

            if (isPlayerInRoom)
            {
                EnsureVFXSpawned(room, state);

                if (!state.isFullyGrown)
                {
                    UpdateVFXGrowth(state);
                }
            }
            else
            {
                if (state.zoneInstance != null)
                {
                    UpdateVFXShrink(state);
                }
            }
        }
    }

    void EnsureVFXSpawned(RoomData room, ToxicRoomState state)
    {
        if (state.zoneInstance != null || toxicVFXPrefab == null) return;

        Transform parent = objectContainer != null ? objectContainer : transform;
        Vector3 worldPos = GridToWorldPosition(room.center.x, room.center.y);

        state.zoneInstance = Instantiate(toxicVFXPrefab, worldPos, Quaternion.identity, parent);
        state.zoneInstance.name = $"ToxicVFX_Room_{room.roomID}";

        state.particleSys = state.zoneInstance.GetComponentInChildren<ParticleSystem>();
        state.areaCollider = state.zoneInstance.GetComponent<CircleCollider2D>();

        state.currentScale = 0.05f;
        state.isFullyGrown = false;

        state.zoneInstance.transform.localScale = Vector3.one;

        ApplyScaleToComponents(state, state.currentScale);

        if (state.particleSys != null && !state.particleSys.isPlaying)
        {
            state.particleSys.Play();
        }
    }

    void UpdateVFXGrowth(ToxicRoomState state)
    {
        if (state.zoneInstance == null) return;

        if (state.currentScale < targetRadius)
        {
            float speed = targetRadius / Mathf.Max(growDuration, 0.01f);
            state.currentScale = Mathf.MoveTowards(state.currentScale, targetRadius, speed * Time.deltaTime);

            ApplyScaleToComponents(state, state.currentScale);

            if (Mathf.Approximately(state.currentScale, targetRadius) || state.currentScale >= targetRadius)
            {
                state.currentScale = targetRadius;
                state.isFullyGrown = true;
            }
        }
    }

    void UpdateVFXShrink(ToxicRoomState state)
    {
        if (state.zoneInstance == null) return;

        float speed = targetRadius / Mathf.Max(shrinkDuration, 0.01f);
        state.currentScale = Mathf.MoveTowards(state.currentScale, 0f, speed * Time.deltaTime);

        ApplyScaleToComponents(state, state.currentScale);

        if (state.currentScale <= 0.05f)
        {
            Destroy(state.zoneInstance);
            state.zoneInstance = null;
            state.isFullyGrown = false;
        }
    }

    private void ApplyScaleToComponents(ToxicRoomState state, float scaleValue)
    {
        // 1. Gán chính xác bán kính chuẩn cho CircleCollider2D
        if (state.areaCollider != null)
        {
            state.areaCollider.radius = scaleValue;
        }

        // 2. Nhân thêm hệ số (mặc định = 3.0) để phóng to bán kính Particle cho bằng với Collider
        if (state.particleSys != null)
        {
            var shape = state.particleSys.shape;
            shape.radius = scaleValue * particleScaleMultiplier;
        }
    }

    Vector3 GridToWorldPosition(int x, int y)
    {
        Vector3Int cellPosition = new Vector3Int(x, y, 0);
        if (mapGenerator != null && mapGenerator.groundTilemaps != null && mapGenerator.groundTilemaps.Count > 0 && mapGenerator.groundTilemaps[0] != null)
        {
            return mapGenerator.groundTilemaps[0].GetCellCenterWorld(cellPosition);
        }
        return new Vector3(x + 0.5f, y + 0.5f, 0f);
    }
}