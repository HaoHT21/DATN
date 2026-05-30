using UnityEngine;

namespace SceneTransition
{
    /// <summary>
    /// Gắn vào Button UI (OnClick) hoặc gọi LoadTargetScene() từ script khác.
    /// </summary>
    public sealed class SceneTransitionButton : MonoBehaviour
    {
        [SerializeField] private string targetSceneName = "Sanh";
        [SerializeField] private SceneTransitionMode loadMode = SceneTransitionMode.Asynchronous;
        [SerializeField] private bool disableButtonWhileTransitioning = true;

        private UnityEngine.UI.Button _button;

        private void Awake()
        {
            _button = GetComponent<UnityEngine.UI.Button>();
        }

        private void OnEnable()
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.OnTransitionStarted += HandleTransitionStarted;
                SceneTransitionManager.Instance.OnTransitionCompleted += HandleTransitionCompleted;
            }
        }

        private void OnDisable()
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.OnTransitionStarted -= HandleTransitionStarted;
                SceneTransitionManager.Instance.OnTransitionCompleted -= HandleTransitionCompleted;
            }
        }

        /// <summary>Gán cho UnityEvent của Button.</summary>
        public void LoadTargetScene()
        {
            if (SceneTransitionManager.Instance == null)
            {
                Debug.LogError("[SceneTransitionButton] Không tìm thấy SceneTransitionManager trong scene.");
                return;
            }

            var request = new SceneTransitionRequest(targetSceneName, loadMode);
            SceneTransitionManager.Instance.LoadScene(request);
        }

        public void SetTargetScene(string sceneName)
        {
            targetSceneName = sceneName;
        }

        private void HandleTransitionStarted(string _)
        {
            if (disableButtonWhileTransitioning && _button != null)
                _button.interactable = false;
        }

        private void HandleTransitionCompleted(string _)
        {
            if (_button != null)
                _button.interactable = true;
        }
    }
}
