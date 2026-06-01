using System;

/// <summary>
/// Nguồn dữ liệu máu — Player, Enemy, Boss đều có thể implement.
/// Health Bar chỉ lắng nghe sự kiện, không gọi TakeDamage.
/// </summary>
public interface IHealthProvider
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsDead { get; }

    /// <summary>current, max, delta (âm = sát thương, dương = hồi máu).</summary>
    event Action<HealthChangeInfo> OnHealthChanged;
}

/// <summary>Payload sự kiện thay đổi máu.</summary>
public readonly struct HealthChangeInfo
{
    public readonly int Current;
    public readonly int Max;
    public readonly int Delta;

    public bool WasDamage => Delta < 0;
    public bool WasHeal => Delta > 0;
    public float Normalized => Max > 0 ? (float)Current / Max : 0f;

    public HealthChangeInfo(int current, int max, int delta)
    {
        Current = current;
        Max = max;
        Delta = delta;
    }
}
