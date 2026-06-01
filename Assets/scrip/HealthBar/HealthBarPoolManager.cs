using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pool thanh máu enemy — giảm Instantiate/Destroy khi spawn wave lớn.
/// Gắn một instance duy nhất trong scene (hoặc DontDestroyOnLoad).
/// </summary>
public class HealthBarPoolManager : MonoBehaviour
{
    public static HealthBarPoolManager Instance { get; private set; }

    [Header("Prefab & Pool")]
    [SerializeField] private WorldHealthBarFollow worldBarPrefab;
    [SerializeField] private Transform poolRoot;
    [SerializeField] private int prewarmCount = 16;
    [SerializeField] private int maxPoolSize = 64;

    [Header("Tối ưu")]
    [Tooltip("Một Canvas World Space chung cho tất cả bar — ít batch hơn nhiều Canvas riêng.")]
    [SerializeField] private bool useSharedWorldCanvas = true;
    [SerializeField] private Canvas sharedWorldCanvas;

    private readonly Queue<WorldHealthBarFollow> _pool = new Queue<WorldHealthBarFollow>();
    private readonly HashSet<WorldHealthBarFollow> _rented = new HashSet<WorldHealthBarFollow>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (poolRoot == null)
        {
            var rootGo = new GameObject("HealthBarPool");
            rootGo.transform.SetParent(transform);
            poolRoot = rootGo.transform;
        }

        if (useSharedWorldCanvas && sharedWorldCanvas == null)
            sharedWorldCanvas = CreateSharedCanvas();

        Prewarm();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private Canvas CreateSharedCanvas()
    {
        var go = new GameObject("SharedWorldHealthCanvas");
        go.transform.SetParent(transform);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        go.AddComponent<CanvasScaler>();
        return canvas;
    }

    private void Prewarm()
    {
        if (worldBarPrefab == null)
            return;

        for (int i = 0; i < prewarmCount; i++)
            Return(CreateInstance());
    }

    public WorldHealthBarFollow Rent(EnemyHealthBarAnchor anchor, IHealthProvider provider, Vector3 offset)
    {
        if (worldBarPrefab == null || anchor == null)
            return null;

        WorldHealthBarFollow bar = _pool.Count > 0 ? _pool.Dequeue() : CreateInstance();

        if (useSharedWorldCanvas && sharedWorldCanvas != null)
            bar.transform.SetParent(sharedWorldCanvas.transform, false);

        bar.gameObject.SetActive(true);
        bar.SetWorldOffset(offset);
        bar.Initialize(anchor.transform, provider);
        _rented.Add(bar);
        return bar;
    }

    public void Return(WorldHealthBarFollow bar)
    {
        if (bar == null)
            return;

        _rented.Remove(bar);
        bar.Clear();
        bar.gameObject.SetActive(false);
        bar.transform.SetParent(poolRoot, false);

        if (_pool.Count < maxPoolSize)
            _pool.Enqueue(bar);
        else
            Destroy(bar.gameObject);
    }

    private WorldHealthBarFollow CreateInstance()
    {
        var instance = Instantiate(worldBarPrefab, poolRoot);
        instance.gameObject.SetActive(false);
        return instance;
    }

    /// <summary>Số bar đang dùng — debug / profiler.</summary>
    public int ActiveCount => _rented.Count;
    public int PooledCount => _pool.Count;
}
