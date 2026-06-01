using UnityEngine;

/// <summary>
/// Gắn trên Enemy — đăng ký thanh máu world khi spawn, hủy khi chết.
/// </summary>
[RequireComponent(typeof(EnemyHealth))]
public class EnemyHealthBarAnchor : MonoBehaviour
{
    [SerializeField] private Vector3 barOffset = new Vector3(0f, 1.2f, 0f);

    private EnemyHealth _health;
    private WorldHealthBarFollow _activeBar;

    private void Awake()
    {
        _health = GetComponent<EnemyHealth>();
    }

    private void OnEnable()
    {
        if (HealthBarPoolManager.Instance != null)
            _activeBar = HealthBarPoolManager.Instance.Rent(this, _health, barOffset);
    }

    private void OnDisable()
    {
        if (HealthBarPoolManager.Instance != null && _activeBar != null)
            HealthBarPoolManager.Instance.Return(_activeBar);
        _activeBar = null;
    }
}
