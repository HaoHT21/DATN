using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;

public class Spawner : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Player Prefab (Dung de tham chieu)")]
    public NetworkPrefabRef playerPrefab;

    private NetworkRunner _runner;

    private void Start()
    {
        _runner = GetComponent<NetworkRunner>();

        if (_runner == null)
        {
            _runner = FindFirstObjectByType<NetworkRunner>();
        }

        if (_runner != null)
        {
            _runner.AddCallbacks(this);
            Debug.Log("<color=green>===> Spawner tai MainMenu da dang ky Callback thanh cong!</color>");
        }
    }

    // =========================================================================
    // SỰ KIỆN KHI CÓ NGƯỜI CHƠI THAM GIA (TẠI MAIN MENU)
    // =========================================================================
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        // TUYỆT ĐỐI ĐỂ TRỐNG: Không gọi lệnh runner.Spawn ở đây!
        // Để tránh việc nhân vật bị sinh nhầm ra sảnh MainMenu.
        // Khi NetworkRunner load sang map Sanh, AutoSpawner bên map đó sẽ tự quét danh sách và Spawn sau.
        Debug.Log($"[MainMenu] Player {player.PlayerId} da ket noi. Cho load sang map choi để AutoSpawner xu ly...");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }

    private void OnDestroy()
    {
        if (_runner != null)
        {
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