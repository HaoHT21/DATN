using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveGameService
{
    private const string FileName = "savegame.json";

    public static string SaveFilePath =>
        Path.Combine(Application.persistentDataPath, FileName);

    public static bool HasSave()
    {
        try
        {
            return File.Exists(SaveFilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveGame] HasSave failed: {e.Message}");
            return false;
        }
    }

    public static void DeleteSave()
    {
        try
        {
            if (File.Exists(SaveFilePath))
                File.Delete(SaveFilePath);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveGame] DeleteSave failed: {e.Message}");
        }
    }

    public static SaveData Load()
    {
        try
        {
            if (!File.Exists(SaveFilePath))
                return null;

            string json = File.ReadAllText(SaveFilePath);
            if (string.IsNullOrWhiteSpace(json))
                return null;

            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveGame] Load failed: {e.Message}");
            return null;
        }
    }

    public static void Save(SaveData data)
    {
        if (data == null)
            return;

        try
        {
            data.savedAtUtcIso = DateTime.UtcNow.ToString("o");
            string json = JsonUtility.ToJson(data, prettyPrint: true);
            File.WriteAllText(SaveFilePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveGame] Save failed: {e.Message}");
        }
    }

    public static SaveData CaptureFromCurrentScene()
    {
        var data = new SaveData();
        data.sceneName = SceneManager.GetActiveScene().name;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            data.SetPlayerPosition(playerObj.transform.position);

        PlayerStats stats = UnityEngine.Object.FindFirstObjectByType<PlayerStats>();
        if (stats != null)
            data.score = stats.Score;

        CaptureSaveables(data);
        return data;
    }

    private static void CaptureSaveables(SaveData data)
    {
        if (data == null)
            return;

        SaveableEntity[] entities = UnityEngine.Object.FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);
        if (entities == null || entities.Length == 0)
            return;

        var records = new List<SaveData.SaveRecord>(entities.Length);

        foreach (SaveableEntity entity in entities)
        {
            if (entity == null || string.IsNullOrWhiteSpace(entity.Id))
                continue;

            ISaveable saveable = entity.GetComponent<ISaveable>();
            if (saveable == null)
                continue;

            object state = saveable.CaptureState();
            if (state == null)
                continue;

            string json = JsonUtility.ToJson(state);
            records.Add(new SaveData.SaveRecord
            {
                id = entity.Id,
                type = state.GetType().AssemblyQualifiedName,
                json = json
            });
        }

        data.records = records;
    }
}

