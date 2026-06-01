using UnityEngine;
using UnityEngine.EventSystems;

namespace SceneTransition
{
    /// <summary>
    /// Chặn input qua overlay CanvasGroup + tạm vô hiệu EventSystem.
    /// </summary>
    public sealed class TransitionInputBlocker : ITransitionInputBlocker
    {
        private readonly CanvasGroup _overlayGroup;
        private EventSystem _eventSystem;
        private bool _eventSystemWasEnabled;

        public TransitionInputBlocker(CanvasGroup overlayGroup)
        {
            _overlayGroup = overlayGroup;
        }

        public void SetBlocked(bool blocked)
        {
            if (_overlayGroup != null)
            {
                _overlayGroup.blocksRaycasts = blocked;
                _overlayGroup.interactable = blocked;
            }

            if (blocked)
                DisableEventSystem();
            else
                RestoreEventSystem();
        }

        private void DisableEventSystem()
        {
            _eventSystem = EventSystem.current;
            if (_eventSystem == null)
                return;

            _eventSystemWasEnabled = _eventSystem.enabled;
            _eventSystem.enabled = false;
        }

        private void RestoreEventSystem()
        {
            if (_eventSystem == null)
                return;

            _eventSystem.enabled = _eventSystemWasEnabled;
            _eventSystem = null;
        }
    }
}
