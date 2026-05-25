using Fusion;
using Fusion.Sockets;
using UnityEngine;
using System;
using System.Collections.Generic;

public class LocalInputHandler : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("UI Setup")]
    public GameObject roomEntryPrefab;
    [HideInInspector] public RectTransform roomListContent;

    private NetworkRunner _runner;

    // --- BIẾN TẠM ĐỂ LƯU CÚ CLICK ---
    private bool _isAttackConsumed = false;
    private bool _isRPressedConsumed = false;

    private void Start()
    {
        _runner = GetComponent<NetworkRunner>();
        if (_runner != null) _runner.AddCallbacks(this);
    }

    private void Update()
    {
        // 1. Thu thập cú click chuột/phím K
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.K))
        {
            _isAttackConsumed = true;
        }

        // 2. Thu thập cú nhấn phím R
        if (Input.GetKeyDown(KeyCode.R))
        {
            _isRPressedConsumed = true;
        }
    }

    // THU THẬP INPUT TỪ NGƯỜI CHƠI GỬI LÊN SERVER
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var myInput = new PlayerInputData();

        myInput.MovementDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // QUAN TRỌNG: Gán giá trị từ biến tạm vào dữ liệu mạng
        myInput.IsAttackPressed = _isAttackConsumed;
        myInput.IsRPressed = _isRPressedConsumed; // <--- TRUNG THIẾU DÒNG NÀY NÈ!

        input.Set(myInput);

        // Reset các biến tạm về false để không bị lặp lại hành động
        _isAttackConsumed = false;
        _isRPressedConsumed = false; // <--- TRUNG THIẾU DÒNG NÀY NỮA!
    }

    // --- CÁC HÀM CALLBACKS GIỮ NGUYÊN ---
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (roomListContent == null) return;
        foreach (Transform child in roomListContent) Destroy(child.gameObject);
        foreach (var session in sessionList)
        {
            if (!session.IsVisible || !session.IsOpen) continue;
            GameObject entry = Instantiate(roomEntryPrefab, roomListContent);
            entry.GetComponent<RectTransform>().localScale = Vector3.one;
            string roomPassword = "";
            if (session.Properties.TryGetValue("pw", out var prop)) roomPassword = prop.ToString();
            if (entry.TryGetComponent<RoomEntryUI>(out var entryUI))
            {
                entryUI.Setup(session.Name, roomPassword);
            }
        }
    }

    private void OnDestroy()
    {
        if (_runner != null) _runner.RemoveCallbacks(this);
    }

    #region Callbacks Bắt Buộc (Giữ nguyên)
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress address, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}