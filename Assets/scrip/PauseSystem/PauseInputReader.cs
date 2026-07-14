using UnityEngine;

/// <summary>
/// Đọc phím Pause (ESC). Không đụng Time.timeScale hay UI — chỉ phát tín hiệu (SRP).
/// Gắn cùng GameObject với PauseMenuController hoặc GameManager.
/// </summary>
public sealed class PauseInputReader : MonoBehaviour
{
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;
    [SerializeField] private bool inputEnabled = true;

    private IPauseInputGate _gate;

    public void Initialize(IPauseInputGate gate)
    {
        _gate = gate ?? new DefaultPauseInputGate();
    }

    private void Awake()
    {
        if (_gate == null)
            _gate = new DefaultPauseInputGate();
    }

    /// <summary>True đúng một frame khi người chơi yêu cầu toggle pause.</summary>
    public bool WasPausePressedThisFrame()
    {
        if (!inputEnabled)
            return false;

        if (_gate != null && !_gate.CanTogglePause)
            return false;

        return Input.GetKeyDown(pauseKey);
    }
}
