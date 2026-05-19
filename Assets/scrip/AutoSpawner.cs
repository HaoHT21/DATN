using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

public class AutoSpawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Player Prefab")]
    public NetworkPrefabRef playerPrefab;

    [Header("Spawn Points")]
    public Transform[] spawnPoints;

    private NetworkRunner _runner;

    // FIX LOGIC: Bỏ 'static' để danh sách tự thiết lập lại sạch sẽ mỗi khi chuyển sang Map mới
    private HashSet<PlayerRef> _spawnedPlayers = new HashSet<PlayerRef>();

    private void Start()
    {
        _runner = FindFirstObjectByType<NetworkRunner>();

        if (_runner == null)
        {
            Debug.LogError("AutoSpawner: Khong tim thay NetworkRunner trong Scene hiện tại!");
            return;
        }

        _runner.AddCallbacks(this);

        // Nếu là Server/Host, tự động quét và sinh nhân vật cho những người đã có sẵn trong phòng khi load Map
        if (_runner.IsServer)
        {
            foreach (var p in _runner.ActivePlayers)
            {
                if (!_spawnedPlayers.Contains(p))
                {
                    ExecuteSpawn(_runner, p);
                }
            }
        }
    }

    // Sự kiện tự động kích hoạt khi có người chơi mới bấm Join vào phòng mạng
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return; // Chỉ Server mới có quyền sinh nhân vật
        if (_spawnedPlayers.Contains(player)) return;

        ExecuteSpawn(runner, player);
    }

    private void ExecuteSpawn(NetworkRunner runner, PlayerRef player)
    {
        if (!playerPrefab.IsValid || spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("AutoSpawner: Prefab hoac Spawn Points chua duoc keo tha thiet lap trong Inspector!");
            return;
        }

        _spawnedPlayers.Add(player);

        // Thuật toán chia đều vị trí xuất hiện dựa trên ID người chơi để tránh trùng tọa độ gây kẹt nhân vật
        int index = player.RawEncoded % spawnPoints.Length;
        Vector3 spawnPos = spawnPoints[index].position;

        // Sinh nhân vật trên mạng và cấp quyền điều khiển (Input Authority) trực tiếp cho Player đó
        NetworkObject obj = runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);

        // Đồng bộ hóa Object này chính là nhân vật đại diện cho người chơi này
        runner.SetPlayerObject(player, obj);

        Debug.Log($"<color=cyan>===> [SPAWN SUCCESS] Da sinh nhan vat cho Player {player.PlayerId} tai diem point {index}</color>");
    }

    // Sự kiện tự động kích hoạt khi có ai đó ngắt kết nối hoặc thoát game
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!runner.IsServer) return;

        // Nếu người chơi thoát, tìm nhân vật của họ trên mạng và xóa bỏ hoàn toàn để tránh lỗi rác dữ liệu
        if (runner.TryGetPlayerObject(player, out NetworkObject playerObj))
        {
            runner.Despawn(playerObj);
        }

        _spawnedPlayers.Remove(player);
        Debug.Log($"[AutoSpawner] Player {player.PlayerId} da roi phong. Da xoa du lieu nhan vat.");
    }

    private void OnDestroy()
    {
        if (_runner != null)
        {
            // Hủy đăng ký lắng nghe khi Object này bị hủy để tránh rò rỉ bộ nhớ (Memory Leak)
            _runner.RemoveCallbacks(this);
        }
    }

    // =========================================================================
    // HỆ THỐNG CALLBACKS BẮT BUỘC CỦA FUSION (ĐỂ TRỐNG ĐỂ TRÁNH LỖI BIÊN DỊCH)
    // =========================================================================
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress address, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}