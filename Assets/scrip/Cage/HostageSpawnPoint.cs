using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Điểm spawn con tin đã giải cứu trong scene đích.
/// Gắn vào scene an toàn (ví dụ Sanh) và cấu hình prefab hiển thị.
/// </summary>
public class HostageSpawnPoint : MonoBehaviour
{
    private static readonly Dictionary<string, HostageSpawnPoint> Registry = new Dictionary<string, HostageSpawnPoint>();

    [Header("Cấu hình")]
    public string spawnPointId = "default";
    public GameObject rescuedHostagePrefab;

    private readonly HashSet<string> _spawnedHostageIds = new HashSet<string>();

    public static HostageSpawnPoint Find(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        Registry.TryGetValue(id, out HostageSpawnPoint point);
        return point;
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(spawnPointId))
            return;

        Registry[spawnPointId] = this;
    }

    private void OnDisable()
    {
        if (string.IsNullOrEmpty(spawnPointId))
            return;

        if (Registry.TryGetValue(spawnPointId, out HostageSpawnPoint point) && point == this)
            Registry.Remove(spawnPointId);
    }

    public bool TrySpawn(string hostageId)
    {
        if (string.IsNullOrEmpty(hostageId) || _spawnedHostageIds.Contains(hostageId))
            return false;

        if (rescuedHostagePrefab == null)
        {
            Debug.LogWarning($"[HostageSpawnPoint] '{spawnPointId}' thiếu rescuedHostagePrefab.");
            return false;
        }

        Instantiate(rescuedHostagePrefab, transform.position, transform.rotation, transform);
        _spawnedHostageIds.Add(hostageId);
        return true;
    }
}
