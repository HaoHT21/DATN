using SceneTransition;

/// <summary>
/// Kiểm tra nhanh xem input gameplay có được phép xử lý hay không.
/// </summary>
public static class GameplayInputGate
{
    public static bool CanProcessInput =>
        !IsTransitioning
        && !IsPaused
        && !IsDialogueActive;

    private static bool IsTransitioning =>
        SceneTransitionManager.Instance != null
        && SceneTransitionManager.Instance.IsTransitioning;

    private static bool IsPaused =>
        GameManager.Instance != null
        && GameManager.Instance.IsGameplayPaused;

    private static bool IsDialogueActive =>
        DialogueManager.Instance != null
        && DialogueManager.Instance.IsDialogueActive;
}
