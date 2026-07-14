using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class SaveableEntity : MonoBehaviour
{
    [SerializeField] private string id;

    public string Id => id;

    private void Awake()
    {
        EnsureId();
    }

    private void OnValidate()
    {
        EnsureId();
    }

    /// <summary>
    /// Gán id ổn định (ưu tiên nếu đang trống hoặc forceOverwrite).
    /// </summary>
    public void EnsureId(string preferredId = null, bool forceOverwrite = false)
    {
        if (!forceOverwrite && !string.IsNullOrWhiteSpace(id))
            return;

        if (!string.IsNullOrWhiteSpace(preferredId))
            id = preferredId;
        else if (string.IsNullOrWhiteSpace(id))
            id = Guid.NewGuid().ToString("N");
    }
}
