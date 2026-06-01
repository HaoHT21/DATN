using UnityEngine;

/// <summary>
/// Đặt trên prefab thanh máu World Space — theo vị trí anchor trên đầu enemy.
/// </summary>
[DisallowMultipleComponent]
public class WorldHealthBarFollow : MonoBehaviour
{
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.2f, 0f);
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private CanvasGroup canvasGroup;

    private Transform _target;
    private Camera _cam;
    private HealthBarUI _barUI;
    private IHealthProvider _provider;

    private void Awake()
    {
        _barUI = GetComponentInChildren<HealthBarUI>();
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetWorldOffset(Vector3 offset) => worldOffset = offset;

    public void Initialize(Transform target, IHealthProvider provider, Camera cam = null)
    {
        Clear();

        _target = target;
        _provider = provider;
        _cam = cam != null ? cam : Camera.main;

        if (_provider != null && _barUI != null)
        {
            _provider.OnHealthChanged += OnHealthChanged;
            _barUI.SetHealth(_provider.CurrentHealth, _provider.MaxHealth);
            UpdateVisibility();
        }
    }

    public void Clear()
    {
        if (_provider != null)
            _provider.OnHealthChanged -= OnHealthChanged;

        _target = null;
        _provider = null;
    }

    private void LateUpdate()
    {
        if (_target == null)
            return;

        transform.position = _target.position + worldOffset;

        if (faceCamera && _cam != null)
            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
    }

    private void OnHealthChanged(HealthChangeInfo info)
    {
        if (_barUI != null)
            _barUI.ApplyChange(info);

        UpdateVisibility();
    }

    private void UpdateVisibility()
    {
        if (!hideWhenFull || _provider == null)
            return;

        bool show = _provider.CurrentHealth < _provider.MaxHealth && !_provider.IsDead;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = show ? 1f : 0f;
            canvasGroup.blocksRaycasts = false;
        }
        else if (_barUI != null)
        {
            _barUI.SetVisible(show);
        }
    }

    private void OnDisable()
    {
        Clear();
    }
}
