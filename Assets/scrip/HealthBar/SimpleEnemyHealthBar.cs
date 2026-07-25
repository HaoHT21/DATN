using UnityEngine;

/// <summary>
/// Thanh máu world đơn giản cho enemy — tự tạo sprite BG + Fill, không cần Canvas/Pool.
/// Gắn cùng object có EnemyHeath (hoặc bất kỳ IHealthProvider).
/// </summary>
[DisallowMultipleComponent]
public class SimpleEnemyHealthBar : MonoBehaviour
{
    [Header("Vị trí & kích thước")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private float width = 1f;
    [SerializeField] private float height = 0.12f;
    [SerializeField] private int sortingOrder = 50;

    [Header("Màu")]
    [SerializeField] private Color backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
    [SerializeField] private Color fillColor = new Color(0.85f, 0.15f, 0.15f, 1f);
    [SerializeField] private Color fillLowColor = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField] [Range(0f, 1f)] private float lowHealthThreshold = 0.3f;

    [Header("Hiển thị")]
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool hideWhenDead = true;

    private IHealthProvider _provider;
    private Transform _root;
    private Transform _fill;
    private SpriteRenderer _bgRenderer;
    private SpriteRenderer _fillRenderer;
    private static Sprite _sharedSprite;

    private void Awake()
    {
        _provider = GetComponent<IHealthProvider>();
        if (_provider == null)
        {
            Debug.LogWarning($"[{nameof(SimpleEnemyHealthBar)}] Không tìm thấy IHealthProvider trên {name}.", this);
            enabled = false;
            return;
        }

        BuildVisual();
        Refresh(_provider.CurrentHealth, _provider.MaxHealth);
    }

    private void OnEnable()
    {
        if (_provider == null)
            return;

        _provider.OnHealthChanged += OnHealthChanged;
        Refresh(_provider.CurrentHealth, _provider.MaxHealth);
    }

    private void OnDisable()
    {
        if (_provider != null)
            _provider.OnHealthChanged -= OnHealthChanged;

        if (_root != null)
            _root.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_root == null)
            return;

        _root.position = transform.position + offset;
        // Giữ hướng cố định (2D) — không flip theo scale X của enemy
        _root.rotation = Quaternion.identity;
        _root.localScale = Vector3.one;
    }

    private void OnHealthChanged(HealthChangeInfo info)
    {
        Refresh(info.Current, info.Max);
    }

    private void Refresh(int current, int max)
    {
        if (_fill == null)
            return;

        max = Mathf.Max(1, max);
        float normalized = Mathf.Clamp01((float)current / max);
        float fillWidth = width * normalized;

        // Scale theo % máu, neo mép trái thanh BG
        _fill.localScale = new Vector3(fillWidth, height * 0.7f, 1f);
        _fill.localPosition = new Vector3((-width + fillWidth) * 0.5f, 0f, -0.01f);

        if (_fillRenderer != null)
            _fillRenderer.color = normalized <= lowHealthThreshold ? fillLowColor : fillColor;

        bool visible = true;
        if (hideWhenDead && (_provider == null || _provider.IsDead || current <= 0))
            visible = false;
        else if (hideWhenFull && current >= max)
            visible = false;

        if (_root != null)
            _root.gameObject.SetActive(visible);
    }

    private void BuildVisual()
    {
        Sprite sprite = GetOrCreateSprite();

        var rootGo = new GameObject("SimpleHealthBar");
        rootGo.transform.SetParent(null);
        _root = rootGo.transform;

        var bgGo = new GameObject("Background");
        bgGo.transform.SetParent(_root, false);
        bgGo.transform.localScale = new Vector3(width, height, 1f);
        _bgRenderer = bgGo.AddComponent<SpriteRenderer>();
        _bgRenderer.sprite = sprite;
        _bgRenderer.color = backgroundColor;
        _bgRenderer.sortingOrder = sortingOrder;

        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(_root, false);
        _fill = fillGo.transform;
        _fillRenderer = fillGo.AddComponent<SpriteRenderer>();
        _fillRenderer.sprite = sprite;
        _fillRenderer.color = fillColor;
        _fillRenderer.sortingOrder = sortingOrder + 1;
    }

    private static Sprite GetOrCreateSprite()
    {
        if (_sharedSprite != null)
            return _sharedSprite;

        // 1x1 trắng → pixelsPerUnit = 1 → sprite đúng 1 unit world trước khi scale
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;

        _sharedSprite = Sprite.Create(
            tex,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f);
        _sharedSprite.name = "SimpleHealthBarSprite";
        return _sharedSprite;
    }

    private void OnDestroy()
    {
        if (_root != null)
            Destroy(_root.gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = Application.isPlaying && _root != null
            ? _root.position
            : transform.position + offset;
        Gizmos.DrawWireCube(center, new Vector3(width, height, 0.01f));
    }
#endif
}
