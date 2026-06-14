using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý trạng thái giải cứu con tin xuyên scene.
/// Lưu số lượng đã giải cứu và spawn con tin tại scene đích.
/// </summary>
public class HostageRescueManager : MonoBehaviour
{
    public static HostageRescueManager Instance { get; private set; }

    private const string CountKey = "HostageRescueCount";
    private const string RecordsKey = "HostageRescueRecords";

    public int RescuedCount { get; private set; }

    public event Action<int> OnRescueCountChanged;

    private readonly HashSet<string> _rescuedIds = new HashSet<string>();
    private readonly Dictionary<string, HostageTransferRecord> _records = new Dictionary<string, HostageTransferRecord>();
    private readonly HashSet<string> _spawnedInCurrentScene = new HashSet<string>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoBootstrap()
    {
        EnsureInstance();
    }

    public static HostageRescueManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var existing = FindFirstObjectByType<HostageRescueManager>();
        if (existing != null)
            return existing;

        var go = new GameObject(nameof(HostageRescueManager));
        return go.AddComponent<HostageRescueManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadSavedData();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;

        if (Instance == this)
            Instance = null;
    }

    public bool IsAlreadyRescued(string hostageId)
    {
        return !string.IsNullOrEmpty(hostageId) && _rescuedIds.Contains(hostageId);
    }

    public void RegisterRescue(string hostageId, string targetSceneName, string spawnPointId)
    {
        if (string.IsNullOrEmpty(hostageId) || _rescuedIds.Contains(hostageId))
            return;

        _rescuedIds.Add(hostageId);
        RescuedCount++;
        _records[hostageId] = new HostageTransferRecord
        {
            hostageId = hostageId,
            targetSceneName = targetSceneName,
            spawnPointId = spawnPointId
        };

        SaveData();
        OnRescueCountChanged?.Invoke(RescuedCount);

        if (SceneManager.GetActiveScene().name == targetSceneName)
            TrySpawnHostage(hostageId);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _spawnedInCurrentScene.Clear();
        SpawnHostagesForScene(scene.name);
    }

    private void SpawnHostagesForScene(string sceneName)
    {
        foreach (KeyValuePair<string, HostageTransferRecord> pair in _records)
        {
            if (pair.Value.targetSceneName == sceneName)
                TrySpawnHostage(pair.Key);
        }
    }

    private void TrySpawnHostage(string hostageId)
    {
        if (_spawnedInCurrentScene.Contains(hostageId))
            return;

        if (!_records.TryGetValue(hostageId, out HostageTransferRecord record))
            return;

        HostageSpawnPoint spawnPoint = HostageSpawnPoint.Find(record.spawnPointId);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[HostageRescue] Không tìm thấy spawn point '{record.spawnPointId}' cho con tin '{hostageId}'.");
            return;
        }

        if (!spawnPoint.TrySpawn(hostageId))
            return;

        _spawnedInCurrentScene.Add(hostageId);
    }

    private void LoadSavedData()
    {
        _rescuedIds.Clear();
        _records.Clear();
        RescuedCount = PlayerPrefs.GetInt(CountKey, 0);

        if (!PlayerPrefs.HasKey(RecordsKey))
            return;

        string raw = PlayerPrefs.GetString(RecordsKey, string.Empty);
        if (string.IsNullOrEmpty(raw))
            return;

        string[] entries = raw.Split('|');
        foreach (string entry in entries)
        {
            if (string.IsNullOrEmpty(entry))
                continue;

            string[] parts = entry.Split(':');
            if (parts.Length < 3)
                continue;

            var record = new HostageTransferRecord
            {
                hostageId = parts[0],
                targetSceneName = parts[1],
                spawnPointId = parts[2]
            };

            _rescuedIds.Add(record.hostageId);
            _records[record.hostageId] = record;
        }

        RescuedCount = _rescuedIds.Count;
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(CountKey, RescuedCount);
        PlayerPrefs.SetString(RecordsKey, BuildRecordsPayload());
        PlayerPrefs.Save();
    }

    private string BuildRecordsPayload()
    {
        var builder = new StringBuilder();

        foreach (HostageTransferRecord record in _records.Values)
        {
            if (builder.Length > 0)
                builder.Append('|');

            builder.Append(record.hostageId)
                .Append(':')
                .Append(record.targetSceneName)
                .Append(':')
                .Append(record.spawnPointId);
        }

        return builder.ToString();
    }

    [Serializable]
    private struct HostageTransferRecord
    {
        public string hostageId;
        public string targetSceneName;
        public string spawnPointId;
    }
}
