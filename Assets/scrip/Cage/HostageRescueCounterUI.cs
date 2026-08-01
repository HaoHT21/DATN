using TMPro;
using UnityEngine;

/// <summary>
/// Hiển thị số lượng con tin đã giải cứu thành công.
/// </summary>
public class HostageRescueCounterUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI countText;
    public string displayFormat = "Con tin đã cứu: {0}/8";

    private void OnEnable()
    {
        HostageRescueManager manager = HostageRescueManager.EnsureInstance();
        manager.OnRescueCountChanged += HandleCountChanged;
        HandleCountChanged(manager.RescuedCount);
    }

    private void OnDisable()
    {
        if (HostageRescueManager.Instance != null)
            HostageRescueManager.Instance.OnRescueCountChanged -= HandleCountChanged;
    }

    private void HandleCountChanged(int count)
    {
        if (countText == null)
            return;

        countText.text = string.Format(displayFormat, count);
    }
}
