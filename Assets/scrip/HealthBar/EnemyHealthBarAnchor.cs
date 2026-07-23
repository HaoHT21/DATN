using UnityEngine;

/// <summary>
/// Gắn trên Enemy — đăng ký thanh máu world (pool) khi spawn, hủy khi chết.
/// Hỗ trợ EnemyHeath / EnemyHealth (mọi IHealthProvider trên cùng object).
/// </summary>
public class EnemyHealthBarAnchor : MonoBehaviour
{
    [SerializeField] private Vector3 barOffset = new Vector3(0f, 1.2f, 0f);

    private IHealthProvider _health;
    private WorldHealthBarFollow _activeBar;

    private void Awake()
    {
        _health = GetComponent<IHealthProvider>();
    }

    private void OnEnable()
    {
        if (_health == null)
            _health = GetComponent<IHealthProvider>();

        if (_health == null || HealthBarPoolManager.Instance == null)
            return;

        _activeBar = HealthBarPoolManager.Instance.Rent(this, _health, barOffset);
    }

    private void OnDisable()
    {
        if (HealthBarPoolManager.Instance != null && _activeBar != null)
            HealthBarPoolManager.Instance.Return(_activeBar);
        _activeBar = null;
    }
}
