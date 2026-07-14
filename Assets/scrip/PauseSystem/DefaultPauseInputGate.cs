using SceneTransition;
using UnityEngine;

/// <summary>
/// Chặn ESC / pause khi đang hội thoại, chuyển cảnh, hoặc game over / bad ending.
/// </summary>
public sealed class DefaultPauseInputGate : IPauseInputGate
{
    public bool CanTogglePause
    {
        get
        {
            if (SceneTransitionManager.Instance != null && SceneTransitionManager.Instance.IsTransitioning)
                return false;

            if (DialogueManager.Instance != null && DialogueManager.Instance.IsDialogueActive)
                return false;

            return true;
        }
    }
}
