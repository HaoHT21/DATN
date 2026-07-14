using System;

/// <summary>
/// Hợp đồng điều khiển tạm dừng gameplay — UI và Input phụ thuộc abstraction này (DIP).
/// </summary>
public interface IGamePauseService
{
    bool IsPaused { get; }
    GameState CurrentState { get; }
    event Action<GameState> OnStateChanged;

    void Pause();
    void Resume();
    void TogglePause();
}
