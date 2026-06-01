using UnityEngine;

/// <summary>
/// Gắn HealthBarUI với một IHealthProvider (Player HUD hoặc bar cố định trên Canvas).
/// </summary>
[DisallowMultipleComponent]
public class HealthBarBinder : MonoBehaviour
{
    [SerializeField] private HealthBarUI healthBarUI;
    [SerializeField] private MonoBehaviour healthProviderComponent;
    [SerializeField] private bool bindOnEnable = true;
    [SerializeField] private bool unbindOnDisable = true;

    private IHealthProvider _provider;

    private void Reset()
    {
        healthBarUI = GetComponent<HealthBarUI>();
    }

    private void OnEnable()
    {
        if (bindOnEnable)
            TryBind();
    }

    private void OnDisable()
    {
        if (unbindOnDisable)
            Unbind();
    }

    public void Bind(IHealthProvider provider)
    {
        Unbind();
        if (provider == null || healthBarUI == null)
            return;

        _provider = provider;
        _provider.OnHealthChanged += OnHealthChanged;
        healthBarUI.SetHealth(_provider.CurrentHealth, _provider.MaxHealth);
    }

    public void BindFromInspector()
    {
        if (healthProviderComponent is IHealthProvider provider)
            Bind(provider);
        else
            Debug.LogWarning($"{nameof(HealthBarBinder)}: Component không implement {nameof(IHealthProvider)}.", this);
    }

    private void TryBind()
    {
        if (_provider != null)
            return;

        if (healthProviderComponent is IHealthProvider provider)
            Bind(provider);
    }

    private void Unbind()
    {
        if (_provider == null)
            return;

        _provider.OnHealthChanged -= OnHealthChanged;
        _provider = null;
    }

    private void OnHealthChanged(HealthChangeInfo info)
    {
        healthBarUI.ApplyChange(info);
    }
}
