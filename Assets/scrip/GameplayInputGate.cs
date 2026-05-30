using SceneTransition;

/// <summary>
/// Kiểm tra nhanh xem input gameplay có được phép xử lý hay không.
/// </summary>
public static class GameplayInputGate
{
    public static bool CanProcessInput =>
        (SceneTransitionManager.Instance == null || !SceneTransitionManager.Instance.IsTransitioning)
        && (GameManager.Instance == null || !GameManager.Instance.IsGameplayPaused);
}
