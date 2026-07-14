/// <summary>
/// Điều kiện cho phép mở / đóng Pause Menu bằng phím (ISP: tách khỏi logic pause).
/// </summary>
public interface IPauseInputGate
{
    bool CanTogglePause { get; }
}
