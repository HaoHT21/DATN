using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using TMPro;
using UnityEngine;

public class NetworkMenuManager : MonoBehaviour, INetworkRunnerCallbacks
{
    [Header("Network Config")]
    public NetworkRunner runnerPrefab;

    [Header("UI Canvas Groups")]
    public Canvas createRoomCanvas;
    public Canvas roomListCanvas;
    public Canvas passwordJoinCanvas;

    [Header("Room List Content Link")]
    public RectTransform roomListContent;

    [Header("Input Fields")]
    public TMP_InputField roomNameInput;
    public TMP_InputField passwordInput;
    public TMP_InputField joinPassInput;

    private NetworkRunner _runner;
    private string _selectedRoomName;
    private string _targetPassword;

    private void Awake()
    {
        if (createRoomCanvas) createRoomCanvas.gameObject.SetActive(false);
        if (roomListCanvas) roomListCanvas.gameObject.SetActive(false);
        if (passwordJoinCanvas) passwordJoinCanvas.gameObject.SetActive(false);
    }

    public void OnClickHost() => createRoomCanvas.gameObject.SetActive(true);
    public void OnClickJoin() { roomListCanvas.gameObject.SetActive(true); StartLobby(); }

    private async void StartLobby()
    {
        InitNetworkRunnerIfNeeded();
        if (_runner == null) return;
        await _runner.JoinSessionLobby(SessionLobby.ClientServer);
    }

    public async void ConfirmHost()
    {
        if (string.IsNullOrEmpty(roomNameInput.text)) return;
        InitNetworkRunnerIfNeeded();

        var sceneManager = _runner.GetComponent<NetworkSceneManagerDefault>();
        if (sceneManager == null) sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

        var customProps = new Dictionary<string, SessionProperty>();
        // Lưu mật khẩu dưới dạng string chuẩn
        customProps["pw"] = passwordInput.text.Trim();

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.Host,
            SessionName = roomNameInput.text.Trim(),
            SessionProperties = customProps,
            Scene = SceneRef.FromIndex(1),
            SceneManager = sceneManager
        });
    }

    public void ShowPasswordPrompt(string roomName, string pass)
    {
        _selectedRoomName = roomName;
        _targetPassword = pass;
        passwordJoinCanvas.gameObject.SetActive(true);
        joinPassInput.text = "";
    }

    public async void ConfirmJoinWithPassword()
    {
        // Làm sạch input và mật khẩu đích
        string inputPass = joinPassInput.text.Trim();
        string correctPass = _targetPassword.Trim();

        // So sánh chuỗi chính xác từng ký tự
        if (string.Equals(inputPass, correctPass, StringComparison.Ordinal))
        {
            InitNetworkRunnerIfNeeded();

            var sceneManager = _runner.GetComponent<NetworkSceneManagerDefault>();
            if (sceneManager == null) sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = _selectedRoomName,
                SceneManager = sceneManager
            });

            passwordJoinCanvas.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError($"Sai mật khẩu! Nhập: [{inputPass}] - Đúng: [{correctPass}]");
        }
    }

    private void InitNetworkRunnerIfNeeded()
    {
        if (_runner != null) return;
        if (runnerPrefab == null) return;

        _runner = Instantiate(runnerPrefab);
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        DontDestroyOnLoad(_runner.gameObject);

        var inputHandler = _runner.GetComponent<LocalInputHandler>();
        if (inputHandler != null) inputHandler.roomListContent = roomListContent;
    }

    // --- ĐÂY LÀ NƠI FIX LỖI CỦA BẠN ---
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        if (roomListContent == null) return;

        foreach (Transform child in roomListContent) Destroy(child.gameObject);

        var inputHandler = _runner.GetComponent<LocalInputHandler>();
        if (inputHandler == null || inputHandler.roomEntryPrefab == null) return;

        foreach (var session in sessionList)
        {
            if (session.IsVisible && session.IsOpen)
            {
                GameObject entry = Instantiate(inputHandler.roomEntryPrefab, roomListContent);

                string pass = "";
                // SỬA: Ép kiểu sang (string) thay vì .ToString()
                if (session.Properties.TryGetValue("pw", out var prop))
                {
                    pass = (string)prop;
                }

                var entryUI = entry.GetComponent<RoomEntryUI>();
                if (entryUI != null) entryUI.Setup(session.Name, pass);
            }
        }
    }

    #region Fusion Callbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress address, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    #endregion
}