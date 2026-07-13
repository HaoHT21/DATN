using System.Collections;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime singleton không cần prefab: quản lý luồng Continue/NewGame giữa các scene.
/// </summary>
public sealed class SaveGameRuntime : MonoBehaviour
{
    private static SaveGameRuntime _instance;

    private SaveData _pendingApply;
    private bool _createSaveAfterFirstLoad;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null)
            return;

        var go = new GameObject("SaveGameRuntime");
        DontDestroyOnLoad(go);
        _instance = go.AddComponent<SaveGameRuntime>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void BeginContinue(SaveData data)
    {
        if (_instance == null)
            Bootstrap();

        _instance._pendingApply = data;
        _instance._createSaveAfterFirstLoad = false;
    }

    public static void BeginNewGameCreateSaveAfterLoad()
    {
        if (_instance == null)
            Bootstrap();

        _instance._pendingApply = null;
        _instance._createSaveAfterFirstLoad = true;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (_pendingApply != null)
            StartCoroutine(ApplyWhenReady(_pendingApply));
        else if (_createSaveAfterFirstLoad)
            StartCoroutine(CaptureAndSaveWhenReady());
    }

    private IEnumerator ApplyWhenReady(SaveData data)
    {
        _pendingApply = null;

        const float timeout = 5f;
        float start = Time.unscaledTime;

        GameObject player = null;
        while (player == null && Time.unscaledTime - start < timeout)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                yield return null;
        }

        if (player != null)
            player.transform.position = data.GetPlayerPosition();

        PlayerStats stats = GameObject.FindFirstObjectByType<PlayerStats>();
        if (stats != null)
        {
            stats.Score = data.score;
            stats.UpdateUI();
        }

        ApplySaveables(data);
        SaveGameService.Save(SaveGameService.CaptureFromCurrentScene());
    }

    private IEnumerator CaptureAndSaveWhenReady()
    {
        _createSaveAfterFirstLoad = false;

        const float timeout = 5f;
        float start = Time.unscaledTime;

        GameObject player = null;
        while (player == null && Time.unscaledTime - start < timeout)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                yield return null;
        }

        SaveGameService.Save(SaveGameService.CaptureFromCurrentScene());
    }

    private static void ApplySaveables(SaveData data)
    {
        if (data == null || data.records == null || data.records.Count == 0)
            return;

        SaveableEntity[] entities = UnityEngine.Object.FindObjectsByType<SaveableEntity>(FindObjectsSortMode.None);
        if (entities == null || entities.Length == 0)
            return;

        for (int i = 0; i < data.records.Count; i++)
        {
            SaveData.SaveRecord record = data.records[i];
            if (record == null || string.IsNullOrWhiteSpace(record.id) || string.IsNullOrWhiteSpace(record.type))
                continue;

            SaveableEntity target = null;
            for (int e = 0; e < entities.Length; e++)
            {
                if (entities[e] != null && entities[e].Id == record.id)
                {
                    target = entities[e];
                    break;
                }
            }

            if (target == null)
                continue;

            ISaveable saveable = target.GetComponent<ISaveable>();
            if (saveable == null)
                continue;

            Type stateType = Type.GetType(record.type);
            if (stateType == null)
                continue;

            object stateObj;
            try
            {
                stateObj = Activator.CreateInstance(stateType);
            }
            catch
            {
                continue;
            }

            try
            {
                JsonUtility.FromJsonOverwrite(record.json, stateObj);
            }
            catch
            {
                continue;
            }

            saveable.RestoreState(stateObj);
        }
    }
}

